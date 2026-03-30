using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Command to remove a member from a project.</summary>
public record RemoveProjectMemberCommand(
    ProjectId ProjectId,
    Guid UserId)
    : ICommand<Deleted>;
