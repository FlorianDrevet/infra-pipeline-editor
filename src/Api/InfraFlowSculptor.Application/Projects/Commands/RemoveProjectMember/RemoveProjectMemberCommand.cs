using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Command to remove a member from a project.</summary>
public record RemoveProjectMemberCommand(
    ProjectId ProjectId,
    Guid UserId)
    : IRequest<ErrorOr<Unit>>;
