using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>
/// Pushes the latest project-level generated artifacts (Bicep + infra pipeline + bootstrap to the
/// infrastructure repository, application pipeline files to the application-code repository)
/// in two independent commits, one per repository.
/// </summary>
/// <remarks>
/// Only valid for projects whose <c>LayoutPreset</c> is <c>SplitInfraCode</c>. AllInOne projects
/// must keep using <c>PushProjectGeneratedArtifactsToGitCommand</c>; MultiRepo projects must use
/// the per-config push endpoints.
/// </remarks>
/// <param name="ProjectId">The project to push.</param>
/// <param name="Infra">Optional push target for the infrastructure-flagged repository.</param>
/// <param name="Code">Optional push target for the application-code-flagged repository.</param>
public sealed record PushProjectArtifactsToMultiRepoCommand(
    ProjectId ProjectId,
    RepoPushTarget? Infra,
    RepoPushTarget? Code) : ICommand<PushProjectArtifactsToMultiRepoResult>;

/// <summary>Per-repository push parameters.</summary>
/// <param name="Alias">The expected alias on the project (used for safety check vs resolver).</param>
/// <param name="BranchName">The target branch to create or update.</param>
/// <param name="CommitMessage">The Git commit message.</param>
public sealed record RepoPushTarget(string Alias, string BranchName, string CommitMessage);

/// <summary>Aggregated result of a SplitInfraCode multi-repository push.</summary>
/// <param name="Results">One entry per requested repository, in the order pushed (infra first, then code).</param>
public sealed record PushProjectArtifactsToMultiRepoResult(IReadOnlyList<RepoPushResult> Results);

/// <summary>Outcome of a single repository push.</summary>
/// <param name="Alias">The repository alias that was targeted.</param>
/// <param name="Success">Whether the push succeeded.</param>
/// <param name="BranchUrl">The created/updated branch URL when successful.</param>
/// <param name="CommitSha">The commit SHA when successful.</param>
/// <param name="FileCount">The number of files committed.</param>
/// <param name="ErrorCode">An error code when the push failed.</param>
/// <param name="ErrorDescription">A human-readable error description when the push failed.</param>
public sealed record RepoPushResult(
    string Alias,
    bool Success,
    string? BranchUrl,
    string? CommitSha,
    int FileCount,
    string? ErrorCode,
    string? ErrorDescription);
