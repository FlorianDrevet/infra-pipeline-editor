using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for the project-level MultiRepo bulk push endpoint.</summary>
/// <param name="Configurations">The configuration-level repository targets to push.</param>
public sealed record PushProjectMultiRepoArtifactsRequest(
    [property: Required, MinLength(1)]
    IReadOnlyList<PushProjectMultiRepoConfigurationRequest> Configurations);