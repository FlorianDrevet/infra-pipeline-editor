using InfraFlowSculptor.Contracts.CosmosDbs.Requests;

namespace InfraFlowSculptor.Contracts.CosmosDbs.Responses;

/// <summary>Represents an Azure Cosmos DB account resource.</summary>
/// <param name="Id">Unique identifier of the Cosmos DB account.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Cosmos DB account.</param>
/// <param name="Location">Azure region where the Cosmos DB account is deployed.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record CosmosDbResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<CosmosDbEnvironmentConfigResponse> EnvironmentSettings
);
