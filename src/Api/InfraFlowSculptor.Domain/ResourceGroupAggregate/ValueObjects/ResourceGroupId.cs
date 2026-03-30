using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="ResourceGroup"/>.</summary>
public sealed class ResourceGroupId(Guid value) : Id<ResourceGroupId>(value);