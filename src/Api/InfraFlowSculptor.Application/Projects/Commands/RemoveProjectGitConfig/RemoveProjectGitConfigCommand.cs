using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectGitConfig;

/// <summary>Command to remove the Git repository configuration from a project.</summary>
public record RemoveProjectGitConfigCommand(
    ProjectId ProjectId
) : ICommand<Deleted>;
