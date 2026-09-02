using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Reports the outcome of pushing artifacts to one configuration repository.</summary>
/// <param name="InfrastructureConfigId">The infrastructure configuration whose artifacts were pushed.</param>
/// <param name="RepositoryId">The configuration repository that was targeted.</param>
/// <param name="Success">Whether the push succeeded.</param>
/// <param name="BranchUrl">The created or updated branch URL when successful.</param>
/// <param name="CommitSha">The commit SHA when successful.</param>
/// <param name="FileCount">The number of files committed.</param>
/// <param name="ErrorCode">The error code when the push failed.</param>
/// <param name="ErrorDescription">The human-readable error description when the push failed.</param>
public sealed record ConfigRepositoryPushResult(
    InfrastructureConfigId InfrastructureConfigId,
    InfraConfigRepositoryId RepositoryId,
    bool Success,
    string? BranchUrl,
    string? CommitSha,
    int FileCount,
    string? ErrorCode,
    string? ErrorDescription);