using InfraFlowSculptor.Contracts.ContainerRegistries.Requests;

namespace InfraFlowSculptor.Contracts.ContainerRegistries.Responses;

/// <summary>Represents an Azure Container Registry resource.</summary>
public record ContainerRegistryResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<ContainerRegistryEnvironmentConfigResponse> EnvironmentSettings
);
