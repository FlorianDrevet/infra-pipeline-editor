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
    string? DockerImageName,
    IReadOnlyList<ContainerAppEnvironmentConfigData> EnvironmentSettings);
