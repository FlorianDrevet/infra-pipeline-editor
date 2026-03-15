namespace BicepGenerator.Application.InfrastructureConfig.ReadModels;

public record InfrastructureConfigReadModel(
    Guid Id,
    string Name,
    IReadOnlyList<ResourceGroupReadModel> ResourceGroups,
    IReadOnlyList<EnvironmentDefinitionReadModel> Environments);

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
    IReadOnlyDictionary<string, string> Properties);

public record EnvironmentDefinitionReadModel(
    Guid Id,
    string Name,
    string Location);
