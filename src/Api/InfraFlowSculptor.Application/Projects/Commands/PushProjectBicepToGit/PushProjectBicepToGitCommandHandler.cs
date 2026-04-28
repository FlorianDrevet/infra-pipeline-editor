using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;

/// <summary>
/// Handles the <see cref="PushProjectBicepToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> with <c>config: null</c> and
/// <see cref="ArtifactKind.Infrastructure"/>, resolving to the project's default alias
/// (<c>"default"</c>). Projects with a heterogeneous multi-repo topology must instead use
/// <c>PushProjectGeneratedArtifactsToGit</c>; this handler will fail with
/// <c>GitRouting.AliasNotFound("default")</c> if no default repository exists.
/// </summary>
public sealed class PushProjectBicepToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushProjectBicepToGitCommand, PushBicepToGitResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushProjectBicepToGitCommand command, CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // 2. Load the project
        var project = await projectRepo.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // 3. Resolve the target repository via V2 routing (project-level, default alias).
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Infrastructure);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // 4. Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated Bicep files from Blob Storage (project-level)
        var filesResult = await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"bicep/project/{command.ProjectId.Value}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BicepFilesNotFoundError,
            entityId: command.ProjectId.Value);
        if (filesResult.IsError)
            return filesResult.Errors;

        // 6. Push to Git
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
