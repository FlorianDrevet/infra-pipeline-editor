using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a <see cref="Domain.ProjectAggregate.Project"/> operation.</summary>
public record ProjectResult(
    ProjectId Id,
    Name Name,
    string? Description,
    IReadOnlyList<ProjectMemberResult> Members);
