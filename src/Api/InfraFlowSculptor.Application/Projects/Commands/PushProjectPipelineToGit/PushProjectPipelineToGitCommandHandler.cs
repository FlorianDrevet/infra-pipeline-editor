using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;

/// <summary>Handles the <see cref="PushProjectPipelineToGitCommand"/>.</summary>
public sealed class PushProjectPipelineToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService)
    : ICommandHandler<PushProjectPipelineToGitCommand, PushBicepToGitResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushProjectPipelineToGitCommand command, CancellationToken cancellationToken)
    {
        // 1. Verify write access
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // 2. Load the project
        var project = await projectRepo.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        // 3. Check Git config exists
        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfig = project.GitRepositoryConfiguration;

        // 4. Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated pipeline files from Blob Storage (project-level)
        var filesResult = await GetLatestProjectPipelineFilesAsync(command.ProjectId.Value, cancellationToken);
        if (filesResult.IsError)
            return filesResult.Errors;

        // 6. Push to Git
        var gitProvider = gitProviderFactory.Create(gitConfig.ProviderType);
        return await gitProvider.PushFilesAsync(new GitPushRequest
        {
            Token = secretResult.Value,
            Owner = gitConfig.Owner,
            RepositoryName = gitConfig.RepositoryName,
            BaseBranch = gitConfig.DefaultBranch,
            TargetBranchName = command.BranchName,
            CommitMessage = command.CommitMessage,
            BasePath = gitConfig.PipelineBasePath,
            Files = filesResult.Value,
        }, cancellationToken);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestProjectPipelineFilesAsync(
        Guid projectId, CancellationToken cancellationToken)
    {
        var prefix = $"pipeline/project/{projectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.PipelineFilesNotFoundError(projectId);

        // Find the latest timestamp folder (format: pipeline/project/{projectId}/{yyyyMMddHHmmss}/...)
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

            // Strip the prefix to get relative path: ConfigName/azure-pipelines.yml, etc.
            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        if (files.Count == 0)
            return Errors.Project.PipelineFilesNotFoundError(projectId);

        return GeneratedPipelinePathNormalizer.Normalize(files);
    }
}
