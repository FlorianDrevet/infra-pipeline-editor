using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for the SplitInfraCode dual-repo push endpoint.</summary>
/// <param name="Infra">Optional push parameters for the infrastructure-flagged repository.</param>
/// <param name="Code">Optional push parameters for the application-code-flagged repository.</param>
public sealed record PushMultiRepoArtifactsRequest(
    RepoPushTargetRequest? Infra,
    RepoPushTargetRequest? Code);

/// <summary>Per-repository push target.</summary>
/// <param name="Alias">The expected repository alias on the project.</param>
/// <param name="BranchName">The branch to create or update.</param>
/// <param name="CommitMessage">The commit message.</param>
public sealed record RepoPushTargetRequest(
    [property: Required, StringLength(255)] string Alias,
    [property: Required, StringLength(255)] string BranchName,
    [property: Required, StringLength(2000)] string CommitMessage);
