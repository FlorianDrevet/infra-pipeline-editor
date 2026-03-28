using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to push generated pipeline files to a Git repository.</summary>
public sealed class PushPipelineToGitRequest
{
    /// <summary>The target branch name (will be created or updated).</summary>
    [Required]
    public required string BranchName { get; init; }

    /// <summary>The commit message.</summary>
    [Required]
    public required string CommitMessage { get; init; }
}
