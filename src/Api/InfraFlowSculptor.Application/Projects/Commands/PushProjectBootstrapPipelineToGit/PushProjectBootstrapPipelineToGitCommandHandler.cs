using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBootstrapPipelineToGit;

/// <summary>
/// Handles the <see cref="PushProjectBootstrapPipelineToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> with <c>config: null</c> and
/// <see cref="ArtifactKind.Bootstrap"/>, resolving to the project's infrastructure repository.
/// Explicitly rejected for heterogeneous multi-repo topologies via
/// <see cref="InfraFlowSculptor.Domain.ProjectAggregate.Project.CanGenerateAllFromProjectLevel"/>.
/// Projects with a <c>MultiRepo</c> layout must instead use the config-level
/// <c>PushBootstrapToGitCommand</c> (<c>InfrastructureConfig/Commands/PushBootstrapToGit</c>).
/// </summary>
public sealed class PushProjectBootstrapPipelineToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushProjectBootstrapPipelineToGitCommand, PushBicepToGitResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushProjectBootstrapPipelineToGitCommand command, CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Reject project-level push for heterogeneous multi-repo topologies.
        if (!authResult.Value.CanGenerateAllFromProjectLevel())
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // 2. Load the project
        var project = await projectRepo.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // 3. Resolve the target repository via V2 routing.
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // 4. Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            target.PatSecretName ?? $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated bootstrap pipeline files from Blob Storage.
        // In SplitInfraCode layout, only the infra/ bucket is pushed here — the app/ bucket
        // is pushed by PushProjectArtifactsToMultiRepoCommandHandler to the code repo.
        var isSplit = project.LayoutPreset.Value == LayoutPresetEnum.SplitInfraCode;
        var filesResult = await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"bootstrap/project/{command.ProjectId.Value}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
            entityId: command.ProjectId.Value,
            options: new BlobDownloadHelper.LatestBlobFilesOptions(
                SubPrefix: isSplit ? "infra/" : null,
                CancellationToken: cancellationToken));
        if (filesResult.IsError)
            return filesResult.Errors;

        // 6. Push to Git (bootstrap.pipeline.yml lives at the pipeline base path root, not inside a config sub-folder)
        var gitProvider = gitProviderFactory.Create(target.ProviderType);
        return await gitProvider.PushFilesAsync(new GitPushRequest
        {
            Token = secretResult.Value,
            Owner = target.Owner,
            RepositoryName = target.RepositoryName,
            BaseBranch = target.Branch,
            TargetBranchName = command.BranchName,
            CommitMessage = command.CommitMessage,
            BasePath = target.PipelineBasePath,
            Files = filesResult.Value,
        }, cancellationToken);
    }
}
