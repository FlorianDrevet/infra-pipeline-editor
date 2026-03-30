using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Command to add a member to a project.</summary>
public record AddProjectMemberCommand(
    ProjectId ProjectId,
    Guid UserId,
    string Role)
    : ICommand<ProjectResult>;
