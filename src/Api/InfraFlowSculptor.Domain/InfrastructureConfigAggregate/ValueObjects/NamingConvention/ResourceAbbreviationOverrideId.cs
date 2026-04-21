using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ResourceAbbreviationOverride"/>.</summary>
public sealed class ResourceAbbreviationOverrideId(Guid value) : Id<ResourceAbbreviationOverrideId>(value);
