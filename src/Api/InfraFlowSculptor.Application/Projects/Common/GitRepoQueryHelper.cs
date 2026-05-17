using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Encapsulates the common setup logic shared by git-repo query handlers:
/// access verification, project/config loading, target resolution, secret retrieval, and provider creation.
/// </summary>
public sealed class GitRepoQueryHelper(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory,
    IRepositoryTargetResolver targetResolver) : IGitRepoQueryHelper
{
    /// <inheritdoc />
    public async Task<ErrorOr<GitRepoQueryContext>> ResolveAsync(
        ProjectId projectId,
        InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.InfrastructureConfigId? configId,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(projectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(projectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        Domain.InfrastructureConfigAggregate.InfrastructureConfig? config = null;
        if (configId is not null)
        {
            config = await infraConfigRepository.GetByIdReadOnlyAsync(configId, cancellationToken);
            if (config is null)
                return Errors.InfrastructureConfig.NotFoundError(configId);
        }

        var targetResult = targetResolver.Resolve(project, config, ArtifactKind.ApplicationPipeline);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var gitProvider = gitProviderFactory.Create(target.ProviderType);

        return new GitRepoQueryContext(gitProvider, secretResult.Value, target);
    }
}
