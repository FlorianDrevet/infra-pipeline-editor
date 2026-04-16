using InfraFlowSculptor.Contracts.ContainerRegistries.Requests;

namespace InfraFlowSculptor.Contracts.ContainerRegistries.Responses;

/// <summary>Represents an Azure Container Registry resource.</summary>
public record ContainerRegistryResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyCollection<ContainerRegistryEnvironmentConfigResponse> EnvironmentSettings
);
