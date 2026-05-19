namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Result returned after verifying a Git repository connection and listing branches.</summary>
/// <param name="Owner">Repository owner or Azure DevOps organization/project.</param>
/// <param name="RepositoryName">Repository name.</param>
/// <param name="Branches">Branches returned by the Git provider.</param>
/// <param name="DefaultBranchCandidate">Suggested default branch, usually <c>main</c> when present.</param>
public sealed record ProjectRepositoryConnectionVerificationResult(
    string Owner,
    string RepositoryName,
    IReadOnlyList<GitBranchResult> Branches,
    string? DefaultBranchCandidate);