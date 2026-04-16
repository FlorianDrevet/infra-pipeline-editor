using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;

/// <summary>Handles the <see cref="ListGitBranchesQuery"/>.</summary>
public sealed class ListGitBranchesQueryHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
    : IQueryHandler<ListGitBranchesQuery, IReadOnlyCollection<GitBranchResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyCollection<GitBranchResult>>> Handle(
        ListGitBranchesQuery query, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(query.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(query.ProjectId);

        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfig = project.GitRepositoryConfiguration;

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var gitProvider = gitProviderFactory.Create(gitConfig.ProviderType);
        return await gitProvider.ListBranchesAsync(
            secretResult.Value, gitConfig.Owner, gitConfig.RepositoryName, cancellationToken);
    }
}
