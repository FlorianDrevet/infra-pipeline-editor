using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;

/// <summary>Handles the <see cref="PushProjectGeneratedArtifactsToGitCommand"/>.</summary>
public sealed class PushProjectGeneratedArtifactsToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService)
    : ICommandHandler<PushProjectGeneratedArtifactsToGitCommand, PushBicepToGitResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushProjectGeneratedArtifactsToGitCommand command,
        CancellationToken cancellationToken)
    {
        var authorizationResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authorizationResult.IsError)
            return authorizationResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfiguration = project.GitRepositoryConfiguration;

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}",
            cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var bicepFilesResult = await GetLatestProjectFilesAsync(
            "bicep",
            command.ProjectId.Value,
            Errors.Project.BicepFilesNotFoundError,
            cancellationToken);
        if (bicepFilesResult.IsError)
            return bicepFilesResult.Errors;

        var pipelineFilesResult = await GetLatestProjectFilesAsync(
            "pipeline",
            command.ProjectId.Value,
            Errors.Project.PipelineFilesNotFoundError,
            cancellationToken);
        if (pipelineFilesResult.IsError)
            return pipelineFilesResult.Errors;

        var bootstrapFilesResult = await GetLatestProjectFilesAsync(
            "bootstrap",
            command.ProjectId.Value,
            Errors.Project.BootstrapFilesNotFoundError,
            cancellationToken);
        if (bootstrapFilesResult.IsError)
            return bootstrapFilesResult.Errors;

        var multiScopePushRequest = BuildPushRequest(
            secretResult.Value,
            gitConfiguration.Owner,
            gitConfiguration.RepositoryName,
            gitConfiguration.DefaultBranch,
            command.BranchName,
            command.CommitMessage,
            gitConfiguration.BasePath,
            bicepFilesResult.Value,
            gitConfiguration.PipelineBasePath,
            pipelineFilesResult.Value,
            bootstrapFilesResult.Value);
        if (multiScopePushRequest.IsError)
            return multiScopePushRequest.Errors;

        var gitProvider = gitProviderFactory.Create(gitConfiguration.ProviderType);
        if (gitProvider is not IGitMultiScopePushProviderService multiScopeGitProvider)
        {
            return Errors.GitRepository.PushFailed(
                "The selected Git provider does not support pushing multiple generated artifact roots in a single commit.");
        }

        return await multiScopeGitProvider.PushScopedFilesAsync(multiScopePushRequest.Value, cancellationToken);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestProjectFilesAsync(
        string artifactType,
        Guid projectId,
        Func<Guid, Error> notFoundErrorFactory,
        CancellationToken cancellationToken)
    {
        var prefix = $"{artifactType}/project/{projectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return notFoundErrorFactory(projectId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var blobName in latestBlobs)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is null)
                continue;

            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        return files.Count == 0 ? notFoundErrorFactory(projectId) : files;
    }

    private static ErrorOr<MultiScopeGitPushRequest> BuildPushRequest(
        string token,
        string owner,
        string repositoryName,
        string baseBranch,
        string targetBranchName,
        string commitMessage,
        string? bicepBasePath,
        IReadOnlyDictionary<string, string> bicepFiles,
        string? pipelineBasePath,
        IReadOnlyDictionary<string, string> pipelineFiles,
        IReadOnlyDictionary<string, string> bootstrapFiles)
    {
        var mergedScopes = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        var mergeError = TryMergeFiles(mergedScopes, bicepBasePath, bicepFiles);
        if (mergeError is not null)
            return mergeError.Value;

        mergeError = TryMergeFiles(mergedScopes, pipelineBasePath, pipelineFiles);
        if (mergeError is not null)
            return mergeError.Value;

        mergeError = TryMergeFiles(mergedScopes, pipelineBasePath, bootstrapFiles);
        if (mergeError is not null)
            return mergeError.Value;

        return new MultiScopeGitPushRequest
        {
            Token = token,
            Owner = owner,
            RepositoryName = repositoryName,
            BaseBranch = baseBranch,
            TargetBranchName = targetBranchName,
            CommitMessage = commitMessage,
            Scopes = mergedScopes
                .Select(scope => new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = string.IsNullOrEmpty(scope.Key) ? null : scope.Key,
                    Files = scope.Value,
                })
                .ToList(),
        };
    }

    private static Error? TryMergeFiles(
        IDictionary<string, Dictionary<string, string>> mergedScopes,
        string? basePath,
        IReadOnlyDictionary<string, string> files)
    {
        var normalizedBasePath = NormalizeBasePath(basePath);
        if (!mergedScopes.TryGetValue(normalizedBasePath, out var scopedFiles))
        {
            scopedFiles = new Dictionary<string, string>(StringComparer.Ordinal);
            mergedScopes[normalizedBasePath] = scopedFiles;
        }

        foreach (var (relativePath, content) in files)
        {
            if (scopedFiles.TryGetValue(relativePath, out var existingContent)
                && !string.Equals(existingContent, content, StringComparison.Ordinal))
            {
                var resolvedPath = CombinePath(normalizedBasePath, relativePath);
                return Errors.GitRepository.PushFailed(
                    $"Generated file collision detected for path '{resolvedPath}'.");
            }

            scopedFiles[relativePath] = content;
        }

        return null;
    }

    private static string NormalizeBasePath(string? basePath) =>
        string.IsNullOrWhiteSpace(basePath)
            ? string.Empty
            : basePath.Trim('/');

    private static string CombinePath(string basePath, string relativePath) =>
        string.IsNullOrEmpty(basePath)
            ? relativePath.TrimStart('/')
            : $"{basePath}/{relativePath.TrimStart('/')}";
}