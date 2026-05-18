using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.TestProjectRepositoryConnection;

/// <summary>
/// Handles the <see cref="TestProjectRepositoryConnectionCommand"/>.
/// </summary>
public sealed class TestProjectRepositoryConnectionCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
    : ICommandHandler<TestProjectRepositoryConnectionCommand, TestGitConnectionResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<TestGitConnectionResult>> Handle(
        TestProjectRepositoryConnectionCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var repository = project.Repositories.FirstOrDefault(candidate => candidate.Id == command.RepositoryId);
        if (repository is null)
            return Errors.ProjectRepository.NotFound(command.RepositoryId);

        if (!repository.IsConfigured
            || repository.ProviderType is null
            || string.IsNullOrWhiteSpace(repository.Owner)
            || string.IsNullOrWhiteSpace(repository.RepositoryName))
        {
            return Errors.GitRouting.RepositorySlotNotConfigured(repository.Alias.Value);
        }

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            ProjectGitSecretNames.GetRepositoryPatSecretName(repository.Id),
            cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var gitProvider = gitProviderFactory.Create(repository.ProviderType);
        return await gitProvider.TestConnectionAsync(
            secretResult.Value,
            repository.Owner,
            repository.RepositoryName,
            cancellationToken);
    }
}