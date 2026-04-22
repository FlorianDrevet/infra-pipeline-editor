namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Application-layer model for a Git push request that spans multiple repository roots in a single commit.
/// </summary>
public sealed class MultiScopeGitPushRequest
{
    /// <summary>The authentication token (PAT) for the Git provider.</summary>
    public required string Token { get; init; }

    /// <summary>The repository owner (org/user, or org/project for Azure DevOps).</summary>
    public required string Owner { get; init; }

    /// <summary>The repository name.</summary>
    public required string RepositoryName { get; init; }

    /// <summary>The base branch to branch from (for example, <c>main</c>).</summary>
    public required string BaseBranch { get; init; }

    /// <summary>The target branch name to create or update.</summary>
    public required string TargetBranchName { get; init; }

    /// <summary>The commit message.</summary>
    public required string CommitMessage { get; init; }

    /// <summary>The scoped file sets to push.</summary>
    public required IReadOnlyList<GitPushScope> Scopes { get; init; }

    /// <summary>
    /// Represents one repository root and the generated files that must be written under it.
    /// </summary>
    public sealed class GitPushScope
    {
        /// <summary>The optional repository base path for this scope.</summary>
        public string? BasePath { get; init; }

        /// <summary>The files to push within the scope (relative path to content).</summary>
        public required IReadOnlyDictionary<string, string> Files { get; init; }
    }
}