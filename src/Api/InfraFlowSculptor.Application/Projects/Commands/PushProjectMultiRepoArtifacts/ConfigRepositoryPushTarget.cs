using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Describes a configuration-level repository push.</summary>
/// <param name="RepositoryId">The configuration repository to target.</param>
/// <param name="BranchName">The branch to create or update.</param>
/// <param name="CommitMessage">The commit message to use for the push.</param>
public sealed record ConfigRepositoryPushTarget(
    InfraConfigRepositoryId RepositoryId,
    string BranchName,
    string CommitMessage);