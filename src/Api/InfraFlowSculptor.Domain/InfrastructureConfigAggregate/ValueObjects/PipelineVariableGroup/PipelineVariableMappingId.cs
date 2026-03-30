using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

/// <summary>Strongly-typed identifier for a <see cref="Entities.PipelineVariableMapping"/>.</summary>
public sealed class PipelineVariableMappingId(Guid value) : Id<PipelineVariableMappingId>(value);
