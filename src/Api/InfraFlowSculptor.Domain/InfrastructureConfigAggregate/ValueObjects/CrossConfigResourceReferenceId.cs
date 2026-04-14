using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.CrossConfigResourceReference"/>.</summary>
public sealed class CrossConfigResourceReferenceId(Guid value) : Id<CrossConfigResourceReferenceId>(value);
