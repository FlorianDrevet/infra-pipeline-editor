using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ContainerApp"/> aggregate.</summary>
public interface IContainerAppRepository : IRepository<ContainerApp>
{
    /// <summary>Retrieves all Container Apps belonging to the specified resource group.</summary>
    Task<List<ContainerApp>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
