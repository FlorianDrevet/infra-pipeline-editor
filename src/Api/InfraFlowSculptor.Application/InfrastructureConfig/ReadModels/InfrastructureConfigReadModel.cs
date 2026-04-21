namespace InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

public record InfrastructureConfigReadModel(
    Guid Id,
    string Name,
    Guid ProjectId,
    IReadOnlyList<ResourceGroupReadModel> ResourceGroups,
    IReadOnlyList<EnvironmentDefinitionReadModel> Environments,
    NamingContextReadModel NamingContext,
    IReadOnlyList<RoleAssignmentReadModel> RoleAssignments,
    IReadOnlyList<AppSettingReadModel> AppSettings,
    IReadOnlyList<CrossConfigReferenceReadModel> CrossConfigReferences,
    IReadOnlyDictionary<string, string> ProjectTags,
    IReadOnlyDictionary<string, string> ConfigTags,
    string AppPipelineMode = "Isolated");

public record ResourceGroupReadModel(
    Guid Id,
    string Name,
    string Location,
    IReadOnlyList<AzureResourceReadModel> Resources);

public record AzureResourceReadModel(
    Guid Id,
    string Name,
    string Location,
    string ResourceType,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<ResourceEnvironmentConfigReadModel> EnvironmentConfigs,
    string? AssignedUserAssignedIdentityName = null);

public record ResourceEnvironmentConfigReadModel(
    string EnvironmentName,
    IReadOnlyDictionary<string, string> Properties);

public record EnvironmentDefinitionReadModel(
    Guid Id,
    string Name,
    string ShortName,
    string Location,
    string Prefix,
    string Suffix,
    string? AzureResourceManagerConnection,
    string? SubscriptionId,
    IReadOnlyDictionary<string, string> Tags);

/// <summary>
/// Read model for the project-level naming context
/// containing the default template and per-resource-type overrides.
/// </summary>
public record NamingContextReadModel(
    string? DefaultTemplate,
    IReadOnlyDictionary<string, string> ResourceTemplates,
    IReadOnlyDictionary<string, string> ResourceAbbreviations);

/// <summary>
/// Read model for a role assignment between two Azure resources.
/// </summary>
public record RoleAssignmentReadModel(
    Guid SourceResourceId,
    string SourceResourceName,
    string SourceResourceType,
    string SourceResourceGroupName,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string TargetResourceGroupName,
    string ManagedIdentityType,
    string RoleDefinitionId,
    Guid? UserAssignedIdentityResourceId,
    string? UserAssignedIdentityName,
    string? UserAssignedIdentityResourceGroupName,
    bool IsTargetCrossConfig = false);

/// <summary>
/// Read model for an app setting (environment variable) configured on a compute resource.
/// </summary>
public record AppSettingReadModel(
    Guid ResourceId,
    string ResourceName,
    string ResourceType,
    string Name,
    IReadOnlyDictionary<string, string>? EnvironmentValues,
    Guid? SourceResourceId,
    string? SourceResourceName,
    string? SourceResourceType,
    string? SourceOutputName,
    bool IsOutputReference,
    Guid? KeyVaultResourceId,
    string? KeyVaultResourceName,
    string? SecretName,
    bool IsKeyVaultReference,
    bool IsSourceCrossConfig = false,
    string? SourceResourceGroupName = null,
    string? SecretValueAssignment = null,
    Guid? VariableGroupId = null,
    string? PipelineVariableName = null,
    string? VariableGroupName = null,
    bool IsViaVariableGroup = false);

/// <summary>
/// Read model for a cross-configuration resource reference used in Bicep generation.
/// Contains resolved metadata about the target resource, resource group, and configuration.
/// </summary>
public record CrossConfigReferenceReadModel(
    Guid ReferenceId,
    Guid TargetConfigId,
    string TargetConfigName,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string TargetResourceGroupName,
    string TargetResourceAbbreviation);
