using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

/// <summary>
/// Represents a consumer group within an Azure Event Hub Namespace.
/// </summary>
public sealed class EventHubConsumerGroup : Entity<EventHubConsumerGroupId>
{
    /// <summary>Gets the parent Event Hub Namespace identifier.</summary>
    public AzureResourceId EventHubNamespaceId { get; private set; } = null!;

    /// <summary>Gets the Event Hub name this consumer group belongs to.</summary>
    public string EventHubName { get; private set; } = string.Empty;

    /// <summary>Gets the consumer group name.</summary>
    public string ConsumerGroupName { get; private set; } = string.Empty;

    private EventHubConsumerGroup() { }

    /// <summary>
    /// Creates a new <see cref="EventHubConsumerGroup"/> for the specified namespace.
    /// </summary>
    /// <param name="eventHubNamespaceId">The parent namespace identifier.</param>
    /// <param name="eventHubName">The Event Hub name.</param>
    /// <param name="consumerGroupName">The consumer group name.</param>
    /// <returns>A new <see cref="EventHubConsumerGroup"/> entity.</returns>
    public static EventHubConsumerGroup Create(
        AzureResourceId eventHubNamespaceId,
        string eventHubName,
        string consumerGroupName)
        => new()
        {
            Id = EventHubConsumerGroupId.CreateUnique(),
            EventHubNamespaceId = eventHubNamespaceId,
            EventHubName = eventHubName,
            ConsumerGroupName = consumerGroupName
        };
}
