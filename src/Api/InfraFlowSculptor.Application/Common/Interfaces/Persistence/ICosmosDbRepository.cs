using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="CosmosDb"/> aggregate.</summary>
public interface ICosmosDbRepository : IRepository<CosmosDb>
{
    /// <summary>Retrieves all Cosmos DB accounts belonging to the specified resource group.</summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of matching Cosmos DB aggregates.</returns>
    Task<List<CosmosDb>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
