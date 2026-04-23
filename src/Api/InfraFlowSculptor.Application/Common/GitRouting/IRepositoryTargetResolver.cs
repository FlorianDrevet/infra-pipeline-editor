using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate;

namespace InfraFlowSculptor.Application.Common.GitRouting;

/// <summary>
/// Resolves the concrete repository target (<see cref="ResolvedRepositoryTarget"/>) to use for a
/// generation/push operation. The resolver is a pure synchronous function: it does not perform I/O.
/// The caller is responsible for loading the <see cref="Project"/> with its <c>Repositories</c>
/// collection (e.g. via <c>IProjectRepository.GetByIdWithAllAsync</c>).
/// </summary>
public interface IRepositoryTargetResolver
{
    /// <summary>
    /// Resolves the target repository for an operation.
    /// </summary>
    /// <param name="project">
    /// The project whose repositories should be considered.
    /// Must have its <see cref="Project.Repositories"/> collection loaded.
    /// </param>
    /// <param name="config">
    /// The target infrastructure configuration, or <c>null</c> for a project-level operation.
    /// When <c>null</c>, the resolver falls back to the alias <c>"default"</c>.
    /// When provided, the resolver honors <see cref="InfrastructureConfig.RepositoryBinding"/>.
    /// </param>
    /// <param name="kind">The artifact kind, used to pick the appropriate path override.</param>
    /// <returns>
    /// The resolved target, or a routing error (<c>GitRouting.AliasNotFound</c>,
    /// <c>GitRouting.NoRepositoryConfigured</c>).
    /// </returns>
    ErrorOr<ResolvedRepositoryTarget> Resolve(Project project, global::InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig? config, ArtifactKind kind);
}
