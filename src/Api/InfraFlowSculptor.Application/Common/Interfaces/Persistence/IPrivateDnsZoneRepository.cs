using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository for <see cref="PrivateDnsZone"/> aggregates.</summary>
public interface IPrivateDnsZoneRepository : IRepository<PrivateDnsZone>
{
    /// <summary>Returns all Private DNS Zones belonging to the specified resource group.</summary>
    Task<List<PrivateDnsZone>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
