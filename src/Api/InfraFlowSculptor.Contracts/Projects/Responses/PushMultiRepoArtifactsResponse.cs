namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response body for the SplitInfraCode dual-repo push endpoint.</summary>
/// <param name="Results">Per-repository push outcome (infra first, then code, in push order).</param>
public sealed record PushMultiRepoArtifactsResponse(IReadOnlyList<RepoPushResultResponse> Results);

/// <summary>Outcome of a single repository push.</summary>
/// <param name="Alias">The repository alias that was targeted.</param>
/// <param name="Success">Whether the push succeeded.</param>
/// <param name="BranchUrl">The created or updated branch URL when successful.</param>
/// <param name="CommitSha">The commit SHA when successful.</param>
/// <param name="FileCount">The number of files committed.</param>
/// <param name="ErrorCode">An error code when the push failed.</param>
/// <param name="ErrorDescription">A human-readable error description when the push failed.</param>
public sealed record RepoPushResultResponse(
    string Alias,
    bool Success,
    string? BranchUrl,
    string? CommitSha,
    int FileCount,
    string? ErrorCode,
    string? ErrorDescription);
