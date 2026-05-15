using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.VirtualNetworkEnvironmentSettings"/>.</summary>
public sealed class VirtualNetworkEnvironmentSettingsId(Guid value) : Id<VirtualNetworkEnvironmentSettingsId>(value);
