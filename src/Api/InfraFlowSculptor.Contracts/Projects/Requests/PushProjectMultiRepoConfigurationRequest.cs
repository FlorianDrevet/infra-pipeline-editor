using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Describes one infrastructure configuration included in a MultiRepo bulk push.</summary>
/// <param name="InfrastructureConfigId">The infrastructure configuration identifier.</param>
/// <param name="Repositories">The configuration-owned repositories to target.</param>
public sealed record PushProjectMultiRepoConfigurationRequest(
    [property: Required] Guid InfrastructureConfigId,
    [property: Required, MinLength(1)]
    IReadOnlyList<PushProjectMultiRepoRepositoryRequest> Repositories);