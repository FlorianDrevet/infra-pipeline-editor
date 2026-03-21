using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Command to remove a member from a project.</summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="UserId">The user to remove from the project.</param>
public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId)
    : IRequest<ErrorOr<Deleted>>;
