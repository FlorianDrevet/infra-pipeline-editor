using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;

/// <summary>Command to push the latest generated pipeline files to a Git repository.</summary>
public record PushPipelineToGitCommand(
    Guid InfrastructureConfigId,
    string BranchName,
    string CommitMessage
) : IRequest<ErrorOr<PushBicepToGitResult>>;
