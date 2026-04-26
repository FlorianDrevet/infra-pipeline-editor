using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;

/// <summary>Handles the <see cref="ListCodeRepoBranchesQuery"/>.</summary>
public sealed class ListCodeRepoBranchesQueryHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory,
    IRepositoryTargetResolver targetResolver)
    : IQueryHandler<ListCodeRepoBranchesQuery, IReadOnlyList<GitBranchResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Handle(
        ListCodeRepoBranchesQuery query, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(query.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(query.ProjectId);

        Domain.InfrastructureConfigAggregate.InfrastructureConfig? config = null;
        if (query.ConfigId is not null)
        {
            config = await infraConfigRepository.GetByIdAsync(query.ConfigId, cancellationToken);
            if (config is null)
                return Errors.InfrastructureConfig.NotFoundError(query.ConfigId);
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
        return await gitProvider.ListBranchesAsync(
            secretResult.Value, target.Owner, target.RepositoryName, cancellationToken);
    }
}
