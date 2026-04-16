using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Common;

/// <summary>
/// Application-layer result DTO for the Container Registry aggregate.
/// </summary>
public record ContainerRegistryResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyCollection<ContainerRegistryEnvironmentConfigData> EnvironmentSettings
);
