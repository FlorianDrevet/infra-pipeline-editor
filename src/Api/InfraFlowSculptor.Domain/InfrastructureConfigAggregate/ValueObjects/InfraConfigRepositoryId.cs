using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for an <see cref="Entities.InfraConfigRepository"/>.</summary>
public sealed class InfraConfigRepositoryId(Guid value) : Id<InfraConfigRepositoryId>(value);
