using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 800 â€” Parent / cross-resource reference resolution.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.ResourceIdToInfo"/> and
/// <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300).</para>
/// <para><b>Post-conditions:</b> each module's <c>ParentModuleIdReferences</c>,
/// <c>ParentModuleNameReferences</c> and <c>ExistingResourceIdReferences</c> dictionaries are
/// populated based on the resource's foreign-key properties. Unresolvable references (resource
/// not in the same configuration and not in <c>ExistingResourceReferences</c>) are silently dropped.</para>
/// <para>Special-cased fall-backs:</para>
/// <list type="bullet">
///   <item><description><c>Microsoft.Insights/components</c> and <c>Microsoft.App/managedEnvironments</c> auto-detect a Log Analytics Workspace in the same configuration when their <c>logAnalyticsWorkspaceId</c> property is not set, falling back to a cross-config existing reference when no in-config LAW is found.</description></item>
/// </list>
/// </remarks>
public sealed class ParentReferenceResolutionStage : IBicepGenerationStage
{
    private const string AppServicePlanIdPropertyName = "appServicePlanId";
    private const string AcrLoginServerParameterName = "acrLoginServer";
    private const string AcrPullIdentityIdPropertyName = "acrPullIdentityId";
    private const string AcrUserManagedIdentityIdParameterName = "acrUserManagedIdentityId";
    private const string UserAssignedIdentityResourceIdOutputName = "resourceId";
    private const string ContainerRegistryIdPropertyName = "containerRegistryId";
    private const string ContainerRegistryLoginServerOutputName = "loginServer";
    private const string ContainerRegistryLoginServerPropertyPath = "properties.loginServer";
    private const string ContainerAppEnvironmentIdPropertyName = "containerAppEnvironmentId";
    private const string LogAnalyticsWorkspaceIdPropertyName = "logAnalyticsWorkspaceId";
    private const string SqlServerIdPropertyName = "sqlServerId";
    private const string SqlServerNameReferenceKey = "sqlServerName";

    /// <inheritdoc />
    public int Order => 800;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var resourceIdToInfo = context.ResourceIdToInfo;

        foreach (var item in context.WorkItems)
        {
            var resource = item.Resource;
            var parentModuleIdRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var parentModuleNameRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var parentModuleOutputRefs = new Dictionary<string, (string Name, string ResourceTypeName, string OutputName)>();
            var existingResourceIdRefs = new Dictionary<string, string>();
            var existingResourcePropertyRefs = new Dictionary<string, (string ResourceName, string PropertyPath)>();

            TryResolveIdReference(resource, resourceIdToInfo, AppServicePlanIdPropertyName, parentModuleIdRefs);
            TryResolveIdReference(resource, resourceIdToInfo, ContainerAppEnvironmentIdPropertyName, parentModuleIdRefs);
            ResolveContainerRegistryLoginServerReference(
                item.Module,
                resource,
                context,
                parentModuleOutputRefs,
                existingResourcePropertyRefs);
            ResolveAcrPullIdentityReference(item.Module, resource, context, parentModuleOutputRefs);
            ResolveLogAnalyticsWorkspaceReference(resource, context, parentModuleIdRefs, existingResourceIdRefs);
            TryResolveNameReference(resource, resourceIdToInfo, SqlServerIdPropertyName, SqlServerNameReferenceKey, parentModuleNameRefs);

            var parameters = RemoveDerivedParameters(
                item.Module.Parameters,
                parentModuleOutputRefs.Keys,
                existingResourcePropertyRefs.Keys);

            item.Module = item.Module with
            {
                IdentityKind = item.IdentityKind,
                UsesParameterizedIdentity = item.UsesParameterizedIdentity,
                Parameters = parameters,
                ParentModuleIdReferences = parentModuleIdRefs,
                ParentModuleNameReferences = parentModuleNameRefs,
                ParentModuleOutputReferences = parentModuleOutputRefs,
                ExistingResourceIdReferences = existingResourceIdRefs,
                ExistingResourcePropertyReferences = existingResourcePropertyRefs,
            };
        }
    }

    private static IReadOnlyDictionary<string, object> RemoveDerivedParameters(
        IReadOnlyDictionary<string, object> parameters,
        IEnumerable<string> parentModuleOutputKeys,
        IEnumerable<string> existingResourcePropertyKeys)
    {
        var derivedParameterKeys = new HashSet<string>(parentModuleOutputKeys, StringComparer.OrdinalIgnoreCase);
        derivedParameterKeys.UnionWith(existingResourcePropertyKeys);

        if (derivedParameterKeys.Count == 0)
        {
            return parameters;
        }

        return parameters
            .Where(parameter => !derivedParameterKeys.Contains(parameter.Key))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static void TryResolveIdReference(
        ResourceDefinition resource,
        IReadOnlyDictionary<Guid, (string Name, string ResourceTypeName)> resourceIdToInfo,
        string propertyName,
        IDictionary<string, (string Name, string ResourceTypeName)> target)
    {
        if (resource.Properties.TryGetValue(propertyName, out var idStr)
            && Guid.TryParse(idStr, out var guid)
            && resourceIdToInfo.TryGetValue(guid, out var info))
        {
            target[propertyName] = info;
        }
    }

    private static void TryResolveNameReference(
        ResourceDefinition resource,
        IReadOnlyDictionary<Guid, (string Name, string ResourceTypeName)> resourceIdToInfo,
        string sourcePropertyName,
        string targetKey,
        IDictionary<string, (string Name, string ResourceTypeName)> target)
    {
        if (resource.Properties.TryGetValue(sourcePropertyName, out var idStr)
            && Guid.TryParse(idStr, out var guid)
            && resourceIdToInfo.TryGetValue(guid, out var info))
        {
            target[targetKey] = info;
        }
    }

    private static void ResolveContainerRegistryLoginServerReference(
        GeneratedTypeModule module,
        ResourceDefinition resource,
        BicepGenerationContext context,
        IDictionary<string, (string Name, string ResourceTypeName, string OutputName)> parentModuleOutputRefs,
        IDictionary<string, (string ResourceName, string PropertyPath)> existingResourcePropertyRefs)
    {
        if (!module.Parameters.ContainsKey(AcrLoginServerParameterName))
        {
            return;
        }

        Guid? containerRegistryId = null;
        if (resource.Properties.TryGetValue(ContainerRegistryIdPropertyName, out var containerRegistryIdValue)
            && Guid.TryParse(containerRegistryIdValue, out var parsedContainerRegistryId))
        {
            containerRegistryId = parsedContainerRegistryId;
        }

        if (containerRegistryId is Guid resolvedContainerRegistryId
            && context.ResourceIdToInfo.TryGetValue(resolvedContainerRegistryId, out var containerRegistryInfo)
            && string.Equals(containerRegistryInfo.ResourceTypeName, AzureResourceTypes.ContainerRegistry, StringComparison.OrdinalIgnoreCase))
        {
            parentModuleOutputRefs[AcrLoginServerParameterName] =
                (containerRegistryInfo.Name, containerRegistryInfo.ResourceTypeName, ContainerRegistryLoginServerOutputName);
            return;
        }

        var existingContainerRegistries = context.Request.ExistingResourceReferences
            .Where(reference => reference.ResourceType.Equals(AzureResourceTypes.ArmTypes.ContainerRegistryType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var existingContainerRegistry = containerRegistryId is Guid existingContainerRegistryId
            ? existingContainerRegistries.FirstOrDefault(reference => reference.TargetResourceId == existingContainerRegistryId)
            : null;

        if (existingContainerRegistry is null && existingContainerRegistries.Count == 1)
        {
            existingContainerRegistry = existingContainerRegistries[0];
        }

        if (existingContainerRegistry is null)
        {
            return;
        }

        existingResourcePropertyRefs[AcrLoginServerParameterName] =
            (existingContainerRegistry.ResourceName, ContainerRegistryLoginServerPropertyPath);
    }

    private static void ResolveLogAnalyticsWorkspaceReference(
        ResourceDefinition resource,
        BicepGenerationContext context,
        IDictionary<string, (string Name, string ResourceTypeName)> parentModuleIdRefs,
        IDictionary<string, string> existingResourceIdRefs)
    {
        if (resource.Properties.TryGetValue(LogAnalyticsWorkspaceIdPropertyName, out var lawIdStr)
            && Guid.TryParse(lawIdStr, out var lawGuid)
            && context.ResourceIdToInfo.TryGetValue(lawGuid, out var lawInfo))
        {
            parentModuleIdRefs[LogAnalyticsWorkspaceIdPropertyName] = lawInfo;
            return;
        }

        if (resource.Type is not AzureResourceTypes.ArmTypes.ApplicationInsightsType
            and not AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType)
        {
            return;
        }

        if (parentModuleIdRefs.ContainsKey(LogAnalyticsWorkspaceIdPropertyName))
            return;

        var fallbackLaw = context.Request.Resources.FirstOrDefault(r =>
            r.Type.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType, StringComparison.OrdinalIgnoreCase));
        if (fallbackLaw is not null)
        {
            parentModuleIdRefs[LogAnalyticsWorkspaceIdPropertyName] = (fallbackLaw.Name, AzureResourceTypes.LogAnalyticsWorkspace);
            return;
        }

        var existingLaw = context.Request.ExistingResourceReferences.FirstOrDefault(r =>
            r.ResourceType.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType, StringComparison.OrdinalIgnoreCase));
        if (existingLaw is not null)
        {
            existingResourceIdRefs[LogAnalyticsWorkspaceIdPropertyName] = existingLaw.ResourceName;
        }
    }

    private static void ResolveAcrPullIdentityReference(
        GeneratedTypeModule module,
        ResourceDefinition resource,
        BicepGenerationContext context,
        IDictionary<string, (string Name, string ResourceTypeName, string OutputName)> parentModuleOutputRefs)
    {
        if (!resource.Properties.TryGetValue(AcrPullIdentityIdPropertyName, out var acrPullIdentityIdStr)
            || string.IsNullOrEmpty(acrPullIdentityIdStr)
            || !Guid.TryParse(acrPullIdentityIdStr, out var acrPullIdentityId))
        {
            return;
        }

        if (!context.ResourceIdToInfo.TryGetValue(acrPullIdentityId, out var uaiInfo)
            || !string.Equals(uaiInfo.ResourceTypeName, AzureResourceTypes.UserAssignedIdentity, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Container App modules no longer use acrManagedIdentityClientId — they reference
        // userAssignedIdentityId directly for registry identity (ARM resource ID).
        // FunctionApp modules still use acrUserManagedIdentityId.
        if (!module.Parameters.ContainsKey(AcrUserManagedIdentityIdParameterName))
        {
            return;
        }

        parentModuleOutputRefs[AcrUserManagedIdentityIdParameterName] = (uaiInfo.Name, AzureResourceTypes.UserAssignedIdentity, UserAssignedIdentityResourceIdOutputName);
    }
}
