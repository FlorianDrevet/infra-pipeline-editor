namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Outcome of one configuration-owned repository push.</summary>
/// <param name="InfrastructureConfigId">The infrastructure configuration identifier.</param>
/// <param name="RepositoryId">The configuration repository identifier.</param>
/// <param name="Success">Whether the push succeeded.</param>
/// <param name="BranchUrl">The created or updated branch URL when successful.</param>
/// <param name="CommitSha">The commit SHA when successful.</param>
/// <param name="FileCount">The number of files committed.</param>
/// <param name="ErrorCode">The error code when the push failed.</param>
/// <param name="ErrorDescription">The human-readable error description when the push failed.</param>
public sealed record PushProjectMultiRepoArtifactResultResponse(
    string InfrastructureConfigId,
    string RepositoryId,
    bool Success,
    string? BranchUrl,
    string? CommitSha,
    int FileCount,
    string? ErrorCode,
    string? ErrorDescription);