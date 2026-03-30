using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;

/// <summary>Command to permanently delete a Cosmos DB account resource.</summary>
/// <param name="Id">The Cosmos DB account identifier.</param>
public record DeleteCosmosDbCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
