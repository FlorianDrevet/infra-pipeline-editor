using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.GitRouting;

/// <inheritdoc cref="IRepositoryTargetResolver"/>
public sealed class RepositoryTargetResolver : IRepositoryTargetResolver
{
    /// <inheritdoc />
    public ErrorOr<ResolvedRepositoryTarget> Resolve(Project project, global::InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig? config, ArtifactKind kind)
    {
        ArgumentNullException.ThrowIfNull(project);

        // Today every artifact (Bicep, infra pipeline, bootstrap pipeline) routes to the
        // Infrastructure-flagged repository. ApplicationCode-only routing is reserved for future use.
        var role = RepositoryContentKindsEnum.Infrastructure;

        if (project.LayoutPreset.Value == LayoutPresetEnum.MultiRepo)
        {
            if (config is null || config.Repositories.Count == 0)
                return Errors.GitRouting.NoRepositoryConfigured(project.Id);

            var configRepo = config.Repositories.FirstOrDefault(r => r.ContentKinds.Has(role));
            if (configRepo is null)
                return Errors.GitRouting.AliasNotFound(role.ToString());

            return BuildFromConfigRepository(configRepo);
        }

        if (project.Repositories.Count == 0)
            return Errors.GitRouting.NoRepositoryConfigured(project.Id);

        var projectRepo = project.Repositories.FirstOrDefault(r => r.ContentKinds.Has(role));
        if (projectRepo is null)
            return Errors.GitRouting.AliasNotFound(role.ToString());

        return BuildFromProjectRepository(projectRepo);
    }

    private static ResolvedRepositoryTarget BuildFromProjectRepository(ProjectRepository repo) =>
        new(
            Alias: repo.Alias.Value,
            ProviderType: repo.ProviderType,
            RepositoryUrl: repo.RepositoryUrl,
            Owner: repo.Owner,
            RepositoryName: repo.RepositoryName,
            Branch: repo.DefaultBranch,
            BasePath: null,
            PipelineBasePath: null,
            PatSecretName: null);

    private static ResolvedRepositoryTarget BuildFromConfigRepository(InfraConfigRepository repo) =>
        new(
            Alias: repo.Alias.Value,
            ProviderType: repo.ProviderType,
            RepositoryUrl: repo.RepositoryUrl,
            Owner: repo.Owner,
            RepositoryName: repo.RepositoryName,
            Branch: repo.DefaultBranch,
            BasePath: null,
            PipelineBasePath: null,
            PatSecretName: null);
}
