using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository for <see cref="FrontDoor"/> aggregates.</summary>
public interface IFrontDoorRepository : IRepository<FrontDoor>
{
    /// <summary>Returns all Front Doors belonging to the specified resource group.</summary>
    Task<List<FrontDoor>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
