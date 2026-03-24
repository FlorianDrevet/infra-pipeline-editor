using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ContainerAppEnvironment"/> aggregate.</summary>
public interface IContainerAppEnvironmentRepository : IRepository<ContainerAppEnvironment>
{
    /// <summary>Retrieves all Container App Environments belonging to the specified resource group.</summary>
    Task<List<ContainerAppEnvironment>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
