using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;

/// <summary>
/// Command to push the latest generated bootstrap pipeline file to a Git repository at
/// infrastructure-configuration level (<c>MultiRepo</c> layout, each configuration owns its own
/// repository).
/// </summary>
public record PushBootstrapToGitCommand(
    Guid InfrastructureConfigId,
    string BranchName,
    string CommitMessage
) : ICommand<PushBicepToGitResult>;
