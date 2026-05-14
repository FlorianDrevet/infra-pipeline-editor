using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository for <see cref="VirtualNetwork"/> aggregates.</summary>
public interface IVirtualNetworkRepository : IRepository<VirtualNetwork>
{
    /// <summary>Returns all virtual networks belonging to the specified resource group.</summary>
    Task<List<VirtualNetwork>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
