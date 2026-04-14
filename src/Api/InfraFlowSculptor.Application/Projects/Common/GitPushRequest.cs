namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer model for a Git push request.</summary>
public sealed class GitPushRequest
{
    /// <summary>The authentication token (PAT) for the Git provider.</summary>
    public required string Token { get; init; }

    /// <summary>The repository owner (org/user, or org/project for Azure DevOps).</summary>
    public required string Owner { get; init; }

    /// <summary>The repository name.</summary>
    public required string RepositoryName { get; init; }

    /// <summary>The base branch to branch from (e.g. "main").</summary>
    public required string BaseBranch { get; init; }

    /// <summary>The target branch name to create or update.</summary>
    public required string TargetBranchName { get; init; }

    /// <summary>The commit message.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>Optional sub-path in the repository where files should be placed.</summary>
    public string? BasePath { get; init; }

    /// <summary>The files to push (relative path → content).</summary>
    public required IReadOnlyDictionary<string, string> Files { get; init; }
}
