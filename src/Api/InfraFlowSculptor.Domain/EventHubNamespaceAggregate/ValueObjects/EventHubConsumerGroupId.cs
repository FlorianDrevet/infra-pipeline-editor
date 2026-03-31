using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.EventHubConsumerGroup"/>.</summary>
public sealed class EventHubConsumerGroupId(Guid value) : Id<EventHubConsumerGroupId>(value);
