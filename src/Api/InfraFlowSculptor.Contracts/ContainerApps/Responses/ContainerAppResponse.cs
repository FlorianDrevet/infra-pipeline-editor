using InfraFlowSculptor.Contracts.ContainerApps.Requests;

namespace InfraFlowSculptor.Contracts.ContainerApps.Responses;

/// <summary>Represents an Azure Container App resource.</summary>
public record ContainerAppResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string ContainerAppEnvironmentId,
    string? ContainerRegistryId,
    string? AcrAuthMode,
    string? DockerImageName,
    string? DockerfilePath,
    string? ApplicationName,
    IReadOnlyList<ContainerAppEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
