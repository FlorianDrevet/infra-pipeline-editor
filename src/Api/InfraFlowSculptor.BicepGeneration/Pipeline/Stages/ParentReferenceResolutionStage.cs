using InfraFlowSculptor.GenerationCore;

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
            var existingResourceIdRefs = new Dictionary<string, string>();

            TryResolveIdReference(resource, resourceIdToInfo, AppServicePlanIdPropertyName, parentModuleIdRefs);
            TryResolveIdReference(resource, resourceIdToInfo, ContainerAppEnvironmentIdPropertyName, parentModuleIdRefs);
            ResolveLogAnalyticsWorkspaceReference(resource, context, parentModuleIdRefs, existingResourceIdRefs);
            TryResolveNameReference(resource, resourceIdToInfo, SqlServerIdPropertyName, SqlServerNameReferenceKey, parentModuleNameRefs);

            item.Module = item.Module with
            {
                IdentityKind = item.IdentityKind,
                UsesParameterizedIdentity = item.UsesParameterizedIdentity,
                ParentModuleIdReferences = parentModuleIdRefs,
                ParentModuleNameReferences = parentModuleNameRefs,
                ExistingResourceIdReferences = existingResourceIdRefs,
            };
        }
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

        if (resource.Type is not AzureResourceTypes.ArmTypes.ApplicationInsights
            and not AzureResourceTypes.ArmTypes.ContainerAppEnvironment)
        {
            return;
        }

        if (parentModuleIdRefs.ContainsKey(LogAnalyticsWorkspaceIdPropertyName))
            return;

        var fallbackLaw = context.Request.Resources.FirstOrDefault(r =>
            r.Type.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace, StringComparison.OrdinalIgnoreCase));
        if (fallbackLaw is not null)
        {
            parentModuleIdRefs[LogAnalyticsWorkspaceIdPropertyName] = (fallbackLaw.Name, AzureResourceTypes.LogAnalyticsWorkspace);
            return;
        }

        var existingLaw = context.Request.ExistingResourceReferences.FirstOrDefault(r =>
            r.ResourceType.Equals(AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace, StringComparison.OrdinalIgnoreCase));
        if (existingLaw is not null)
        {
            existingResourceIdRefs[LogAnalyticsWorkspaceIdPropertyName] = existingLaw.ResourceName;
        }
    }
}
