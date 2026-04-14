using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="EventHubNamespace"/> aggregate.</summary>
public interface IEventHubNamespaceRepository : IRepository<EventHubNamespace>
{
    /// <summary>Retrieves all Event Hub Namespaces belonging to the specified resource group.</summary>
    Task<List<EventHubNamespace>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
