using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBootstrapPipelineToGit;

/// <summary>
/// Handles the <see cref="PushProjectBootstrapPipelineToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> with <c>config: null</c> and
/// <see cref="ArtifactKind.Bootstrap"/>, resolving to the project's default alias
/// (<c>"default"</c>). Projects with a heterogeneous multi-repo topology must instead use
/// <c>PushProjectGeneratedArtifactsToGit</c>.
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

        // 2. Load the project
        var project = await projectRepo.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // 3. Resolve the target repository via V2 routing (project-level, default alias).
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        // 4. Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated bootstrap pipeline files from Blob Storage
        var filesResult = await GetLatestBootstrapFilesAsync(command.ProjectId.Value, cancellationToken);
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

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestBootstrapFilesAsync(
        Guid projectId, CancellationToken cancellationToken)
    {
        var prefix = $"bootstrap/project/{projectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.BootstrapFilesNotFoundError(projectId);

        var latestPrefix = allBlobs
            .Select(b => string.Join('/', b.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(b => b.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        var files = new Dictionary<string, string>();
        foreach (var blobName in latestBlobs)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is null) continue;

            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        if (files.Count == 0)
            return Errors.Project.BootstrapFilesNotFoundError(projectId);

        return files;
    }
}
