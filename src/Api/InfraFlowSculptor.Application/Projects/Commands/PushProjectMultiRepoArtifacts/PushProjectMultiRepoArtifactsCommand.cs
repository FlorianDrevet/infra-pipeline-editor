using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>
/// Pushes the generated artifacts for each infrastructure configuration to its configured repositories.
/// </summary>
/// <param name="ProjectId">The project whose configuration artifacts should be pushed.</param>
/// <param name="Configurations">The configuration-level repository push targets.</param>
public sealed record PushProjectMultiRepoArtifactsCommand(
    ProjectId ProjectId,
    IReadOnlyList<InfrastructureConfigPushTarget> Configurations)
    : ICommand<PushProjectMultiRepoArtifactsResult>;