namespace InfraFlowSculptor.Contracts.Git.Responses;

/// <summary>A Git branch returned from a stateless connection verification.</summary>
/// <param name="Name">Branch name.</param>
/// <param name="IsProtected">Whether the branch is protected.</param>
public sealed record VerifiedGitBranchDto(
    string Name,
    bool IsProtected);
