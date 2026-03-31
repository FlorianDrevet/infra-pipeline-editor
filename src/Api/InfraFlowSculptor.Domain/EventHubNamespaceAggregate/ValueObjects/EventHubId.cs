using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.EventHub"/>.</summary>
public sealed class EventHubId(Guid value) : Id<EventHubId>(value);
