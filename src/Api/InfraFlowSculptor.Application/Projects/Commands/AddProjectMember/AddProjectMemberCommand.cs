using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Command to add a member to a project.</summary>
/// <param name="ProjectId">The project to add the member to.</param>
/// <param name="UserId">The user to add as a member.</param>
/// <param name="Role">The role to assign (Owner, Contributor, Reader).</param>
public record AddProjectMemberCommand(Guid ProjectId, Guid UserId, string Role)
    : IRequest<ErrorOr<ProjectResult>>;
