using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Command to update a project member's role.</summary>
public record UpdateProjectMemberRoleCommand(
    ProjectId ProjectId,
    Guid UserId,
    string NewRole)
    : IRequest<ErrorOr<ProjectResult>>;
