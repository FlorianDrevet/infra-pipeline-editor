using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="AppConfiguration"/> aggregate.</summary>
public interface IAppConfigurationRepository : IRepository<AppConfiguration>
{
    /// <summary>Retrieves all App Configurations belonging to the specified resource group.</summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of matching App Configuration aggregates.</returns>
    Task<List<AppConfiguration>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
