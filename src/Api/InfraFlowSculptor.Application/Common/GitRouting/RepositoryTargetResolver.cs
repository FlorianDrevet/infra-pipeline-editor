using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using RepositoryBinding = global::InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.RepositoryBinding;

namespace InfraFlowSculptor.Application.Common.GitRouting;

/// <inheritdoc cref="IRepositoryTargetResolver"/>
public sealed class RepositoryTargetResolver : IRepositoryTargetResolver
{
    private const string DefaultAlias = "default";

    /// <inheritdoc />
    public ErrorOr<ResolvedRepositoryTarget> Resolve(Project project, global::InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig? config, ArtifactKind kind)
    {
        ArgumentNullException.ThrowIfNull(project);

        // 1. Determine the alias to look up: binding (if any) wins, otherwise "default".
        var binding = config?.RepositoryBinding;
        var alias = binding?.Alias.Value ?? DefaultAlias;

        // 2. Locate the matching ProjectRepository (sole source of truth).
        var projectRepo = project.Repositories.FirstOrDefault(r => r.Alias.Value == alias);

        if (projectRepo is not null)
        {
            return BuildFromProjectRepository(projectRepo, binding, kind);
        }

        // 3. No match.
        if (project.Repositories.Count == 0)
        {
            return Errors.GitRouting.NoRepositoryConfigured(project.Id);
        }

        return Errors.GitRouting.AliasNotFound(alias);
    }

    private static ResolvedRepositoryTarget BuildFromProjectRepository(
        ProjectRepository repo,
        RepositoryBinding? binding,
        ArtifactKind kind)
    {
        var branch = binding?.Branch ?? repo.DefaultBranch;
        var (basePath, pipelineBasePath) = ResolvePaths(binding?.InfraPath, binding?.PipelinePath, kind);

        return new ResolvedRepositoryTarget(
            Alias: repo.Alias.Value,
            ProviderType: repo.ProviderType,
            RepositoryUrl: repo.RepositoryUrl,
            Owner: repo.Owner,
            RepositoryName: repo.RepositoryName,
            Branch: branch,
            BasePath: basePath,
            PipelineBasePath: pipelineBasePath,
            // ProjectRepository does not yet expose a PAT secret name — deferred.
            PatSecretName: null);
    }

    private static (string? BasePath, string? PipelineBasePath) ResolvePaths(
        string? bindingInfraPath,
        string? bindingPipelinePath,
        ArtifactKind kind)
    {
        return kind switch
        {
            ArtifactKind.Infrastructure => (bindingInfraPath, null),
            ArtifactKind.Pipeline or ArtifactKind.Bootstrap => (bindingInfraPath, bindingPipelinePath),
            _ => (bindingInfraPath, bindingPipelinePath),
        };
    }
}
