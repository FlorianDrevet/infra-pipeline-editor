using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Command to add a member to a project.</summary>
public record AddProjectMemberCommand(
    ProjectId ProjectId,
    Guid UserId,
    string Role)
    : IRequest<ErrorOr<ProjectResult>>;
