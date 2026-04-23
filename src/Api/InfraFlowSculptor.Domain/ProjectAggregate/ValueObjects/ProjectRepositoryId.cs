using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ProjectRepository"/>.</summary>
public sealed class ProjectRepositoryId(Guid value) : Id<ProjectRepositoryId>(value);
