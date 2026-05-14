using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.VirtualNetworkLink"/>.</summary>
public sealed class VirtualNetworkLinkId(Guid value) : Id<VirtualNetworkLinkId>(value);
