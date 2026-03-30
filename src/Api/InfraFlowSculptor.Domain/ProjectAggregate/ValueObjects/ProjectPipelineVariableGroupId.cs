using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.ProjectPipelineVariableGroup"/>.</summary>
public sealed class ProjectPipelineVariableGroupId(Guid value) : Id<ProjectPipelineVariableGroupId>(value);
