using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ServiceBusTopicSubscription"/>.</summary>
public sealed class ServiceBusTopicSubscriptionId(Guid value) : Id<ServiceBusTopicSubscriptionId>(value);
