using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.Subnet"/>.</summary>
public sealed class SubnetId(Guid value) : Id<SubnetId>(value);
