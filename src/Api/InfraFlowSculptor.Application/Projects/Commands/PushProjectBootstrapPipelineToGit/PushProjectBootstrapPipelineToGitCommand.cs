using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBootstrapPipelineToGit;

/// <summary>Command to push the latest generated bootstrap pipeline file to a Git repository at project level.</summary>
/// <param name="ProjectId">The unique identifier of the project.</param>
/// <param name="BranchName">The target branch name to push the bootstrap pipeline file to.</param>
/// <param name="CommitMessage">The commit message for the push.</param>
public record PushProjectBootstrapPipelineToGitCommand(
    ProjectId ProjectId,
    string BranchName,
    string CommitMessage
) : ICommand<PushBicepToGitResult>;
