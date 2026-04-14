using InfraFlowSculptor.Contracts.ContainerApps.Requests;

namespace InfraFlowSculptor.Contracts.ContainerApps.Responses;

/// <summary>Represents an Azure Container App resource.</summary>
public record ContainerAppResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    Guid ContainerAppEnvironmentId,
    Guid? ContainerRegistryId,
    string? DockerImageName,
    string? DockerfilePath,
    string? ApplicationName,
    IReadOnlyList<ContainerAppEnvironmentConfigResponse> EnvironmentSettings
);
