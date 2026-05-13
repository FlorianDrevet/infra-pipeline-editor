using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Builds <see cref="GenerationRequest"/> instances from infrastructure config read models.
/// Shared between single-config and project-level generation handlers.
/// </summary>
internal static class GenerationRequestBuilder
{
    /// <summary>
    /// Builds a pipeline-focused <see cref="GenerationRequest"/> from the given infrastructure configuration read model.
    /// </summary>
    /// <param name="config">The read model containing all configuration data.</param>
    /// <param name="projectVariableGroups">The project-level pipeline variable groups.</param>
    /// <param name="agentPoolName">The optional self-hosted agent pool name.</param>
    /// <param name="bicepBasePath">The optional base path to generated Bicep files inside the target repository.</param>
    /// <param name="generators">The registered Bicep generators used to derive secure parameter overrides.</param>
    /// <returns>A generation request containing only the data required by pipeline generation.</returns>
    internal static GenerationRequest BuildForPipeline(
        InfrastructureConfigReadModel config,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups,
        string? agentPoolName,
        string? bicepBasePath,
        IEnumerable<IResourceTypeBicepSpecGenerator> generators)
    {
        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);
        var request = BuildCoreRequest(config, mergedAbbreviations, includeExtendedResourceMetadata: false);
        var pipelineVariableGroups = BuildPipelineVariableGroups(config, projectVariableGroups);

        request.PipelineVariableGroups = pipelineVariableGroups;
        request.SecureParameterOverrides = SecureParameterOverrideHelper.DeriveSecureParameterOverrides(
            request.Resources,
            generators,
            config.SecureParameterMappings,
            pipelineVariableGroups);
        request.AgentPoolName = agentPoolName;
        request.BicepBasePath = bicepBasePath;

        return request;
    }

    /// <summary>
    /// Builds a <see cref="GenerationRequest"/> from the given infrastructure configuration read model.
    /// </summary>
    /// <param name="config">The read model containing all configuration data.</param>
    /// <returns>A fully populated generation request.</returns>
    internal static GenerationRequest Build(InfrastructureConfigReadModel config)
    {
        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);
        var request = BuildCoreRequest(config, mergedAbbreviations, includeExtendedResourceMetadata: true);

        var roleAssignments = config.RoleAssignments
            .Select(ra =>
            {
                var targetTypeName = GetResourceTypeName(ra.TargetResourceType);
                var sourceTypeName = GetResourceTypeName(ra.SourceResourceType);
                var roleDef = AzureRoleDefinitionCatalog.GetForResourceType(targetTypeName)
                    .FirstOrDefault(r => r.Id.Equals(ra.RoleDefinitionId, StringComparison.OrdinalIgnoreCase));

                return new RoleAssignmentDefinition
                {
                    SourceResourceName = ra.SourceResourceName,
                    SourceResourceType = ra.SourceResourceType,
                    SourceResourceTypeName = sourceTypeName,
                    SourceResourceGroupName = ra.SourceResourceGroupName,
                    TargetResourceName = ra.TargetResourceName,
                    TargetResourceType = ra.TargetResourceType,
                    TargetResourceGroupName = ra.TargetResourceGroupName,
                    ManagedIdentityType = ra.ManagedIdentityType,
                    RoleDefinitionId = ra.RoleDefinitionId,
                    RoleDefinitionName = roleDef?.Name ?? ra.RoleDefinitionId,
                    RoleDefinitionDescription = roleDef?.Description ?? string.Empty,
                    ServiceCategory = RoleAssignmentModuleTemplates.GetServiceCategory(targetTypeName),
                    TargetResourceTypeName = targetTypeName,
                    TargetResourceAbbreviation = GetResourceAbbreviation(ra.TargetResourceType, request.NamingContext.ResourceAbbreviations),
                    UserAssignedIdentityName = ra.UserAssignedIdentityName,
                    UserAssignedIdentityResourceId = ra.UserAssignedIdentityResourceId,
                    UserAssignedIdentityResourceGroupName = ra.UserAssignedIdentityResourceGroupName,
                    IsTargetCrossConfig = ra.IsTargetCrossConfig,
                };
            })
            .ToList();

        var appSettingDefinitions = config.AppSettings
            .Select(s =>
            {
                var sourceTypeName = s.SourceResourceType is not null
                    ? GetResourceTypeName(s.SourceResourceType)
                    : null;

                string? bicepExpression = null;
                if (sourceTypeName is not null && s.SourceOutputName is not null)
                {
                    var outputDef = ResourceOutputCatalog.FindOutput(sourceTypeName, s.SourceOutputName);
                    bicepExpression = outputDef?.BicepExpression;
                }

                var isSensitiveExport = s.IsKeyVaultReference
                    && s.SourceResourceId is not null
                    && s.SourceOutputName is not null;

                return new AppSettingDefinition
                {
                    Name = s.Name,
                    StaticValue = null,
                    EnvironmentValues = s.EnvironmentValues,
                    SourceResourceName = s.SourceResourceName,
                    SourceOutputName = s.SourceOutputName,
                    SourceResourceTypeName = sourceTypeName,
                    TargetResourceName = s.ResourceName,
                    IsOutputReference = s.IsOutputReference,
                    SourceOutputBicepExpression = bicepExpression,
                    IsKeyVaultReference = s.IsKeyVaultReference,
                    KeyVaultResourceName = s.KeyVaultResourceName,
                    SecretName = s.SecretName,
                    IsSourceCrossConfig = s.IsSourceCrossConfig,
                    SourceResourceGroupName = s.SourceResourceGroupName,
                    IsSensitiveOutputExportedToKeyVault = isSensitiveExport,
                    SecretValueAssignment = s.SecretValueAssignment?.ToString(),
                    IsViaVariableGroup = s.IsViaVariableGroup,
                    PipelineVariableName = s.PipelineVariableName,
                    VariableGroupName = s.VariableGroupName,
                };
            })
            .ToList();

        var existingResourceReferences = config.CrossConfigReferences
            .Select(ccRef =>
            {
                var targetTypeName = GetResourceTypeName(ccRef.TargetResourceType);
                return new ExistingResourceReference
                {
                    ResourceName = ccRef.TargetResourceName,
                    TargetResourceId = ccRef.TargetResourceId,
                    ResourceTypeName = targetTypeName,
                    ResourceType = ccRef.TargetResourceType,
                    ResourceGroupName = ccRef.TargetResourceGroupName,
                    ResourceAbbreviation = ccRef.TargetResourceAbbreviation,
                    SourceConfigName = ccRef.TargetConfigName,
                };
            })
            .ToList();

        var localExistingRefs = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => r.IsExisting)
                .Select(r =>
                {
                    var typeName = GetResourceTypeName(r.ResourceType);
                    return new ExistingResourceReference
                    {
                        ResourceName = r.Name,
                        TargetResourceId = r.Id,
                        ResourceTypeName = typeName,
                        ResourceType = r.ResourceType,
                        ResourceGroupName = rg.Name,
                        ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                        SourceConfigName = string.Empty,
                    };
                }))
            .ToList();

        existingResourceReferences.AddRange(localExistingRefs);

        request.RoleAssignments = roleAssignments;
        request.AppSettings = appSettingDefinitions;
        request.ExistingResourceReferences = existingResourceReferences;
        request.ProjectTags = config.ProjectTags;
        request.ConfigTags = config.ConfigTags;

        return request;
    }

    private static GenerationRequest BuildCoreRequest(
        InfrastructureConfigReadModel config,
        IReadOnlyDictionary<string, string> mergedAbbreviations,
        bool includeExtendedResourceMetadata)
    {
        return new GenerationRequest
        {
            Resources = BuildResources(config, mergedAbbreviations, includeExtendedResourceMetadata),
            ResourceGroups = BuildResourceGroups(config),
            Environments = BuildEnvironments(config),
            EnvironmentNames = config.Environments.Select(environment => environment.Name).ToList(),
            NamingContext = new NamingContext
            {
                DefaultTemplate = config.NamingContext.DefaultTemplate,
                ResourceTemplates = config.NamingContext.ResourceTemplates,
                ResourceAbbreviations = mergedAbbreviations,
            },
        };
    }

    private static List<ResourceDefinition> BuildResources(
        InfrastructureConfigReadModel config,
        IReadOnlyDictionary<string, string> mergedAbbreviations,
        bool includeExtendedResourceMetadata)
    {
        return config.ResourceGroups
            .SelectMany(resourceGroup => resourceGroup.Resources
                .Where(resource => !resource.IsExisting)
                .Select(resource => BuildResourceDefinition(resourceGroup.Name, resource, mergedAbbreviations, includeExtendedResourceMetadata)))
            .ToList();
    }

    private static ResourceDefinition BuildResourceDefinition(
        string resourceGroupName,
        AzureResourceReadModel resource,
        IReadOnlyDictionary<string, string> mergedAbbreviations,
        bool includeExtendedResourceMetadata)
    {
        var definition = new ResourceDefinition
        {
            Name = resource.Name,
            Type = resource.ResourceType,
            ResourceGroupName = resourceGroupName,
            Sku = resource.Properties.GetValueOrDefault("sku", string.Empty),
            Properties = resource.Properties,
            ResourceAbbreviation = GetResourceAbbreviation(resource.ResourceType, mergedAbbreviations),
            EnvironmentConfigs = resource.EnvironmentConfigs
                .ToDictionary(
                    environmentConfig => environmentConfig.EnvironmentName,
                    environmentConfig => environmentConfig.Properties),
            AssignedUserAssignedIdentityName = resource.AssignedUserAssignedIdentityName,
        };

        if (!includeExtendedResourceMetadata)
            return definition;

        definition.ResourceId = resource.Id;
        definition.CustomDomains = (resource.CustomDomains ?? [])
            .Select(customDomain => new CustomDomainDefinition
            {
                EnvironmentName = customDomain.EnvironmentName,
                DomainName = customDomain.DomainName,
                BindingType = customDomain.BindingType,
            })
            .ToList();

        return definition;
    }

    private static List<ResourceGroupDefinition> BuildResourceGroups(InfrastructureConfigReadModel config)
    {
        return config.ResourceGroups
            .Select(resourceGroup => new ResourceGroupDefinition
            {
                Name = resourceGroup.Name,
                Location = resourceGroup.Location,
                ResourceAbbreviation = "rg",
            })
            .ToList();
    }

    private static List<EnvironmentDefinition> BuildEnvironments(InfrastructureConfigReadModel config)
    {
        return config.Environments
            .Select(environment => new EnvironmentDefinition
            {
                Name = environment.Name,
                ShortName = environment.ShortName,
                Location = environment.Location,
                Prefix = environment.Prefix,
                Suffix = environment.Suffix,
                AzureResourceManagerConnection = environment.AzureResourceManagerConnection,
                SubscriptionId = environment.SubscriptionId,
                Tags = environment.Tags,
            })
            .ToList();
    }

    private static List<PipelineVariableGroupDefinition> BuildPipelineVariableGroups(
        InfrastructureConfigReadModel config,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups)
    {
        return projectVariableGroups
            .Select(projectVariableGroup =>
            {
                var mappings = config.AppSettings
                    .Where(appSetting =>
                        appSetting.IsViaVariableGroup
                        && appSetting.VariableGroupId.HasValue
                        && appSetting.VariableGroupId.Value == projectVariableGroup.Id.Value)
                    .Select(appSetting => new PipelineVariableMappingDefinition
                    {
                        PipelineVariableName = appSetting.PipelineVariableName!,
                        BicepParameterName = AppSettingPipelineParameterNameHelper.ResolveBicepParameterName(appSetting),
                    })
                    .ToList();

                return new PipelineVariableGroupDefinition
                {
                    GroupName = projectVariableGroup.GroupName,
                    Mappings = mappings,
                };
            })
            .ToList();
    }

    /// <summary>
    /// Resolves the resource abbreviation from the Azure resource type string,
    /// preferring overrides from the merged abbreviation dictionary.
    /// </summary>
    internal static string GetResourceAbbreviation(
        string azureResourceType,
        IReadOnlyDictionary<string, string> mergedAbbreviations)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return mergedAbbreviations.TryGetValue(typeName, out var abbr)
            ? abbr
            : ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

    /// <summary>
    /// Merges the catalog defaults with user overrides.
    /// Overrides take precedence over catalog entries.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> MergeAbbreviations(
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(ResourceAbbreviationCatalog.GetAll(), StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value;
        }

        return merged;
    }

    /// <summary>
    /// Resolves the simple resource type name from the Azure resource type string
    /// (e.g. <c>"Microsoft.KeyVault/vaults"</c> → <c>"KeyVault"</c>).
    /// </summary>
    internal static string GetResourceTypeName(string azureResourceType) =>
        AzureResourceTypes.GetFriendlyName(azureResourceType);
}
