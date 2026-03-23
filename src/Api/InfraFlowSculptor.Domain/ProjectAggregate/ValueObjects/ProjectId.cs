using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Project"/>.</summary>
public sealed class ProjectId(Guid value) : Id<ProjectId>(value);
