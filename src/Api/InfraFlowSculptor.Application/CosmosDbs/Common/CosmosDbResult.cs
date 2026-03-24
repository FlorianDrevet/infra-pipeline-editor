using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.CosmosDbs.Common;

/// <summary>
/// Application-layer result DTO for the Cosmos DB aggregate.
/// </summary>
public record CosmosDbResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<CosmosDbEnvironmentConfigData> EnvironmentSettings
);
