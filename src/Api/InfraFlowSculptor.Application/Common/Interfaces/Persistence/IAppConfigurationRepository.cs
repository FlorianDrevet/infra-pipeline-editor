using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
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

    /// <summary>Retrieves an App Configuration with its configuration keys and their environment values.</summary>
    /// <param name="id">The App Configuration resource identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The App Configuration aggregate with configuration keys, or null if not found.</returns>
    Task<AppConfiguration?> GetByIdWithConfigurationKeysAsync(AzureResourceId id, CancellationToken cancellationToken);
}
