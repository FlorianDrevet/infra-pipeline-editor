namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Aggregates the outcomes of configuration-level repository pushes.</summary>
/// <param name="Results">The per-repository push outcomes.</param>
public sealed record PushProjectMultiRepoArtifactsResult(IReadOnlyList<ConfigRepositoryPushResult> Results);