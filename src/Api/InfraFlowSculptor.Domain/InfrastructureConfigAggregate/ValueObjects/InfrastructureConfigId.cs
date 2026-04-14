using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for an <see cref="InfrastructureConfig"/>.</summary>
public sealed class InfrastructureConfigId(Guid value) : Id<InfrastructureConfigId>(value);