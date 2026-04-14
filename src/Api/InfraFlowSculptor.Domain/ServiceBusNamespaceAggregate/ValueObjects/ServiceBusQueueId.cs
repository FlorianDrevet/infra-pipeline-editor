using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ServiceBusQueue"/>.</summary>
public sealed class ServiceBusQueueId(Guid value) : Id<ServiceBusQueueId>(value);
