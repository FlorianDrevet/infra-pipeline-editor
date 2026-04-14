using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ServiceBusNamespaceEnvironmentSettings"/>.</summary>
public sealed class ServiceBusNamespaceEnvironmentSettingsId(Guid value) : Id<ServiceBusNamespaceEnvironmentSettingsId>(value);
