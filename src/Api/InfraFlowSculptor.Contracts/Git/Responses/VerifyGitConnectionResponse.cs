namespace InfraFlowSculptor.Contracts.Git.Responses;

/// <summary>Response returned after a stateless Git connection verification.</summary>
/// <param name="Owner">Repository owner or organization/project.</param>
/// <param name="RepositoryName">Repository name.</param>
/// <param name="Branches">Available branches returned by the provider.</param>
/// <param name="DefaultBranchCandidate">Suggested default branch name.</param>
public sealed record VerifyGitConnectionResponse(
    string Owner,
    string RepositoryName,
    IReadOnlyList<VerifiedGitBranchDto> Branches,
    string? DefaultBranchCandidate);
