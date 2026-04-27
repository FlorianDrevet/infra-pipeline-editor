using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;

/// <summary>Handles the <see cref="TestGitConnectionCommand"/>.</summary>
public sealed class TestGitConnectionCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<TestGitConnectionCommand, TestGitConnectionResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<TestGitConnectionResult>> Handle(
        TestGitConnectionCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Infrastructure);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // Test the connection
        var gitProvider = gitProviderFactory.Create(target.ProviderType);
        return await gitProvider.TestConnectionAsync(
            secretResult.Value, target.Owner, target.RepositoryName, cancellationToken);
    }
}
