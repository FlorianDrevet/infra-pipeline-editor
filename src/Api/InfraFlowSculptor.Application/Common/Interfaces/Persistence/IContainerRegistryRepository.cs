using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ContainerRegistry"/> aggregate.</summary>
public interface IContainerRegistryRepository : IRepository<ContainerRegistry>
{
    /// <summary>Retrieves all Container Registries belonging to the specified resource group.</summary>
    Task<List<ContainerRegistry>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
