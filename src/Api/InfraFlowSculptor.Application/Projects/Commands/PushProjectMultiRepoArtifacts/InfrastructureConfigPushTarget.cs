using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Describes the repositories that should receive one infrastructure configuration's artifacts.</summary>
/// <param name="InfrastructureConfigId">The infrastructure configuration to push.</param>
/// <param name="Repositories">The repositories targeted for the configuration.</param>
public sealed record InfrastructureConfigPushTarget(
    InfrastructureConfigId InfrastructureConfigId,
    IReadOnlyList<ConfigRepositoryPushTarget> Repositories);