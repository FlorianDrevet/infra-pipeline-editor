using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;

/// <summary>Command to update an existing Cosmos DB account resource.</summary>
/// <param name="Id">The Cosmos DB account identifier.</param>
/// <param name="Name">The new display name.</param>
/// <param name="Location">The new Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record UpdateCosmosDbCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyCollection<CosmosDbEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<CosmosDbResult>;
