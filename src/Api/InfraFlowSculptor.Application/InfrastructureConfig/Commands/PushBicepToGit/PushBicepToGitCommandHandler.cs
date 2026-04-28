using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBicepToGit;

/// <summary>
/// Handles the <see cref="PushBicepToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> (V2 routing) with <see cref="ArtifactKind.Infrastructure"/>
/// to resolve the target repository from the configuration's <c>RepositoryBinding</c>,
/// falling back to the legacy project-level <c>GitRepositoryConfiguration</c> via the resolver's
/// built-in compatibility path.
/// </summary>
public sealed class PushBicepToGitCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepo,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushBicepToGitCommand, PushBicepToGitResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushBicepToGitCommand command, CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var infraConfigId = new InfrastructureConfigId(command.InfrastructureConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(infraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // 2. Load the infrastructure config and project
        var config = await infraConfigRepo.GetByIdAsync(infraConfigId, cancellationToken);
        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var project = await projectRepo.GetByIdWithAllAsync(config.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(config.ProjectId);

        // 3. Resolve the target repository via V2 routing.
        var targetResult = targetResolver.Resolve(project, config, ArtifactKind.Infrastructure);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // 4. Retrieve the PAT from the centralized Key Vault.
        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated Bicep files from Blob Storage.
        var filesResult = await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"bicep/{command.InfrastructureConfigId}/",
            prefixSegmentCount: 3,
            notFoundErrorFactory: Errors.InfrastructureConfig.BicepFilesNotFoundError,
            entityId: command.InfrastructureConfigId);
        if (filesResult.IsError)
            return filesResult.Errors;

        // 6. Push to Git.
        var gitProvider = gitProviderFactory.Create(target.ProviderType);
        return await gitProvider.PushFilesAsync(new GitPushRequest
        {
            Token = secretResult.Value,
            Owner = target.Owner,
            RepositoryName = target.RepositoryName,
            BaseBranch = target.Branch,
            TargetBranchName = command.BranchName,
            CommitMessage = command.CommitMessage,
            BasePath = target.BasePath,
            Files = filesResult.Value,
        }, cancellationToken);
    }
}
