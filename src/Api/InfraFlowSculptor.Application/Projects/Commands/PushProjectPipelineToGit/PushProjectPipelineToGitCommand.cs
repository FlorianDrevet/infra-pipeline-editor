using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;

/// <summary>Command to push the latest mono-repo generated pipeline files to a Git repository at project level.</summary>
public record PushProjectPipelineToGitCommand(
    ProjectId ProjectId,
    string BranchName,
    string CommitMessage
) : ICommand<PushBicepToGitResult>;
