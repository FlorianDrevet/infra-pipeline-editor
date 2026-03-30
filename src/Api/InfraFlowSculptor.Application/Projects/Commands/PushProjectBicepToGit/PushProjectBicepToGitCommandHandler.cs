using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;

/// <summary>Handles the <see cref="PushProjectBicepToGitCommand"/>.</summary>
public sealed class PushProjectBicepToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService)
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

        // 3. Check Git config exists
        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfig = project.GitRepositoryConfiguration;

        // 4. Retrieve the PAT from the centralized Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        // 5. Retrieve latest generated Bicep files from Blob Storage (project-level)
        var filesResult = await GetLatestProjectBicepFilesAsync(command.ProjectId.Value, cancellationToken);
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
            BasePath = gitConfig.BasePath,
            Files = filesResult.Value,
        }, cancellationToken);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestProjectBicepFilesAsync(
        Guid projectId, CancellationToken cancellationToken)
    {
        var prefix = $"bicep/project/{projectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.BicepFilesNotFoundError(projectId);

        // Find the latest timestamp folder (format: bicep/project/{projectId}/{yyyyMMddHHmmss}/...)
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

            // Strip the prefix to get relative path: Common/types.bicep, ConfigName/main.bicep, etc.
            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        if (files.Count == 0)
            return Errors.Project.BicepFilesNotFoundError(projectId);

        return files;
    }
}
