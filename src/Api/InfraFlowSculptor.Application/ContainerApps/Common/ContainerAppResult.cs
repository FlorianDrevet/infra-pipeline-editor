using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerApps.Common;

/// <summary>
/// Application-layer result DTO for the Container App aggregate.
/// </summary>
public record ContainerAppResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid ContainerAppEnvironmentId,
    Guid? ContainerRegistryId,
    string? AcrAuthMode,
    Guid? AcrPullIdentityId,
    string? DockerImageName,
    bool DockerImageValidated,
    string? DockerfilePath,
    string? ApplicationName,
    string? SourceCodePath,
    PipelineStepOptionsDto? PipelineStepOptions,
    IReadOnlyList<ContainerAppEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false
);
