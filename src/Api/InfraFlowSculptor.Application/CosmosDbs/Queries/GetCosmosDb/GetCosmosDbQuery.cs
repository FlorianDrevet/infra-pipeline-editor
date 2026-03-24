using ErrorOr;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.CosmosDbs.Queries;

/// <summary>Query to retrieve a single Cosmos DB account resource by identifier.</summary>
/// <param name="Id">The Cosmos DB account identifier.</param>
public record GetCosmosDbQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<CosmosDbResult>>;
