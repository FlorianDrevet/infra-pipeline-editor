using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ProjectMember"/>.</summary>
public sealed class ProjectMemberId(Guid value) : Id<ProjectMemberId>(value);
