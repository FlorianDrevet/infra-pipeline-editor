using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

/// <summary>
/// Represents an Event Hub within an Azure Event Hub Namespace.
/// </summary>
public sealed class EventHub : Entity<EventHubId>
{
    /// <summary>Gets the parent Event Hub Namespace identifier.</summary>
    public AzureResourceId EventHubNamespaceId { get; private set; } = null!;

    /// <summary>Gets the Event Hub name.</summary>
    public string Name { get; private set; } = string.Empty;

    private EventHub() { }

    /// <summary>
    /// Creates a new <see cref="EventHub"/> for the specified namespace.
    /// </summary>
    /// <param name="eventHubNamespaceId">The parent namespace identifier.</param>
    /// <param name="name">The Event Hub name.</param>
    /// <returns>A new <see cref="EventHub"/> entity.</returns>
    public static EventHub Create(AzureResourceId eventHubNamespaceId, string name)
        => new()
        {
            Id = EventHubId.CreateUnique(),
            EventHubNamespaceId = eventHubNamespaceId,
            Name = name
        };
}
