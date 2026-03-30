using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.ProjectPipelineVariableMapping"/>.</summary>
public sealed class ProjectPipelineVariableMappingId(Guid value) : Id<ProjectPipelineVariableMappingId>(value);
