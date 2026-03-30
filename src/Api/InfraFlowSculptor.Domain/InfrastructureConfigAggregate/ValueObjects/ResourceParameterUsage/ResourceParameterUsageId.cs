using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;

/// <summary>Strongly-typed identifier for a <see cref="Common.Models.ResourceParameterUsage"/>.</summary>
public sealed class ResourceParameterUsageId(Guid value) : Id<ResourceParameterUsageId>(value);