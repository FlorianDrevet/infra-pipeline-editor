using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository for <see cref="NetworkSecurityGroup"/> aggregates.</summary>
public interface INetworkSecurityGroupRepository : IRepository<NetworkSecurityGroup>
{
    /// <summary>Returns all NSGs belonging to the specified resource group.</summary>
    Task<List<NetworkSecurityGroup>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
