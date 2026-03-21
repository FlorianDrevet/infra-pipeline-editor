using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Command to update the role of a project member.</summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="UserId">The user whose role should be updated.</param>
/// <param name="NewRole">The new role to assign (Owner, Contributor, Reader).</param>
public record UpdateProjectMemberRoleCommand(Guid ProjectId, Guid UserId, string NewRole)
    : IRequest<ErrorOr<ProjectResult>>;
