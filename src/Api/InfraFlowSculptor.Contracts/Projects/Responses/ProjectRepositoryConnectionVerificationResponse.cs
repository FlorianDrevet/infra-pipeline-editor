namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response returned after repository connection verification.</summary>
/// <param name="Owner">Repository owner or Azure DevOps organization/project.</param>
/// <param name="RepositoryName">Repository name.</param>
/// <param name="Branches">Branches returned by the provider.</param>
/// <param name="DefaultBranchCandidate">Suggested default branch.</param>
public sealed record ProjectRepositoryConnectionVerificationResponse(
    string Owner,
    string RepositoryName,
    IReadOnlyList<GitBranchResponse> Branches,
    string? DefaultBranchCandidate);