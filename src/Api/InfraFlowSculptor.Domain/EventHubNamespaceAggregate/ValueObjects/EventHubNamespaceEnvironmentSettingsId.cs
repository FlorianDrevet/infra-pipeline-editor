using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.EventHubNamespaceEnvironmentSettings"/>.</summary>
public sealed class EventHubNamespaceEnvironmentSettingsId(Guid value) : Id<EventHubNamespaceEnvironmentSettingsId>(value);
