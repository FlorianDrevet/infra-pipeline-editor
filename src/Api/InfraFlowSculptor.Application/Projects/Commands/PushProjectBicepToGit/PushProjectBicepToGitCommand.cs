using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;

/// <summary>Command to push the latest mono-repo generated Bicep files to a Git repository at project level.</summary>
public record PushProjectBicepToGitCommand(
    ProjectId ProjectId,
    string BranchName,
    string CommitMessage
) : ICommand<PushBicepToGitResult>;
