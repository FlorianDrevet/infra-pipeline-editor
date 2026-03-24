namespace InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

public record InfrastructureConfigReadModel(
    Guid Id,
    string Name,
    IReadOnlyList<ResourceGroupReadModel> ResourceGroups,
    IReadOnlyList<EnvironmentDefinitionReadModel> Environments,
    NamingContextReadModel NamingContext);

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
    IReadOnlyList<ResourceEnvironmentConfigReadModel> EnvironmentConfigs);

public record ResourceEnvironmentConfigReadModel(
    string EnvironmentName,
    IReadOnlyDictionary<string, string> Properties);

public record EnvironmentDefinitionReadModel(
    Guid Id,
    string Name,
    string ShortName,
    string Location,
    string Prefix,
    string Suffix);

/// <summary>
/// Read model for the project-level naming context
/// containing the default template and per-resource-type overrides.
/// </summary>
public record NamingContextReadModel(
    string? DefaultTemplate,
    IReadOnlyDictionary<string, string> ResourceTemplates);
