using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Common;

/// <summary>
/// Application-layer result DTO for the Container App Environment aggregate.
/// </summary>
public record ContainerAppEnvironmentResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigData> EnvironmentSettings
);
