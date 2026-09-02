namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response body for the project-level MultiRepo bulk push endpoint.</summary>
/// <param name="Results">One independent result per configuration-owned repository push.</param>
public sealed record PushProjectMultiRepoArtifactsResponse(
    IReadOnlyList<PushProjectMultiRepoArtifactResultResponse> Results);