using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBicepToGit;

/// <summary>Command to push the latest generated Bicep files to a Git repository.</summary>
public record PushBicepToGitCommand(
    Guid InfrastructureConfigId,
    string BranchName,
    string CommitMessage
) : ICommand<PushBicepToGitResult>;
