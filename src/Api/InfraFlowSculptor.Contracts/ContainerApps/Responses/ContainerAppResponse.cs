using InfraFlowSculptor.Contracts.Common.Requests;

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
    string? AcrPullIdentityId,
    string? DockerImageName,
    bool DockerImageValidated,
    string? DockerfilePath,
    string? ApplicationName,
    PipelineStepOptionsDto? PipelineStepOptions,
    IReadOnlyList<ContainerAppEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
