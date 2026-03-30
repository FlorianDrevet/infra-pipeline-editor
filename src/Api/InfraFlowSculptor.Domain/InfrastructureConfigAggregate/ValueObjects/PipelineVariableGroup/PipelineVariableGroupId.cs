using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

/// <summary>Strongly-typed identifier for a <see cref="Entities.PipelineVariableGroup"/>.</summary>
public sealed class PipelineVariableGroupId(Guid value) : Id<PipelineVariableGroupId>(value);
