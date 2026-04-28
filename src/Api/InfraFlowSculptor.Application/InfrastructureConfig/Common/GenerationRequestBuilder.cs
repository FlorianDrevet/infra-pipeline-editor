using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
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
    /// Builds a <see cref="GenerationRequest"/> from the given infrastructure configuration read model.
    /// </summary>
    /// <param name="config">The read model containing all configuration data.</param>
    /// <returns>A fully populated generation request.</returns>
    internal static GenerationRequest Build(InfrastructureConfigReadModel config)
    {
        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => !r.IsExisting)
                .Select(r => new ResourceDefinition
                {
                    ResourceId = r.Id,
                    Name = r.Name,
                    Type = r.ResourceType,
                    ResourceGroupName = rg.Name,
                    Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                    Properties = r.Properties,
                    ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                    EnvironmentConfigs = r.EnvironmentConfigs
                        .ToDictionary(
                            ec => ec.EnvironmentName,
                            ec => (IReadOnlyDictionary<string, string>)ec.Properties),
                    AssignedUserAssignedIdentityName = r.AssignedUserAssignedIdentityName,
                    CustomDomains = (r.CustomDomains ?? [])
                        .Select(cd => new CustomDomainDefinition
                        {
                            EnvironmentName = cd.EnvironmentName,
                            DomainName = cd.DomainName,
                            BindingType = cd.BindingType,
                        })
                        .ToList(),
                }))
            .ToList();

        var resourceGroups = config.ResourceGroups
            .Select(rg => new ResourceGroupDefinition
            {
                Name = rg.Name,
                Location = rg.Location,
                ResourceAbbreviation = "rg"
            })
            .ToList();

        var environmentNames = config.Environments.Select(e => e.Name).ToList();

        var environments = config.Environments
            .Select(e => new EnvironmentDefinition
            {
                Name = e.Name,
                ShortName = e.ShortName,
                Location = e.Location,
                Prefix = e.Prefix,
                Suffix = e.Suffix,
                AzureResourceManagerConnection = e.AzureResourceManagerConnection,
                SubscriptionId = e.SubscriptionId,
                Tags = e.Tags,
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = mergedAbbreviations,
        };

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
                    TargetResourceAbbreviation = GetResourceAbbreviation(ra.TargetResourceType, namingContext.ResourceAbbreviations),
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
                        ResourceTypeName = typeName,
                        ResourceType = r.ResourceType,
                        ResourceGroupName = rg.Name,
                        ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                        SourceConfigName = string.Empty,
                    };
                }))
            .ToList();

        existingResourceReferences.AddRange(localExistingRefs);

        return new GenerationRequest
        {
            Resources = resources,
            ResourceGroups = resourceGroups,
            Environments = environments,
            EnvironmentNames = environmentNames,
            NamingContext = namingContext,
            RoleAssignments = roleAssignments,
            AppSettings = appSettingDefinitions,
            ExistingResourceReferences = existingResourceReferences,
            ProjectTags = config.ProjectTags,
            ConfigTags = config.ConfigTags,
        };
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
