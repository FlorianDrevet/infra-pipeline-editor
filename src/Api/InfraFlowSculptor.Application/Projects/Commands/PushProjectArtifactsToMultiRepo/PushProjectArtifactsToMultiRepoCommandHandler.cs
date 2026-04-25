using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>
/// Handles <see cref="PushProjectArtifactsToMultiRepoCommand"/>.
/// Pushes infra artifacts (Bicep + infra pipeline + bootstrap) and/or app pipeline artifacts to
/// the requested repositories in independent commits, returning a per-repo success/error result.
/// </summary>
public sealed class PushProjectArtifactsToMultiRepoCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushProjectArtifactsToMultiRepoCommand, PushProjectArtifactsToMultiRepoResult>
{
    private const string InfraBucket = "infra";
    private const string AppBucket = "app";

    /// <inheritdoc />
    public async Task<ErrorOr<PushProjectArtifactsToMultiRepoResult>> Handle(
        PushProjectArtifactsToMultiRepoCommand command,
        CancellationToken cancellationToken)
    {
        var infraPushTarget = command.Infra;
        var codePushTarget = command.Code;

        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.LayoutPreset.Value != LayoutPresetEnum.SplitInfraCode)
            return Errors.GitRouting.LayoutNotSupportedForMultiRepoPush;

        ResolvedRepositoryTarget? infraTarget = null;
        if (infraPushTarget is not null)
        {
            var infraTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Pipeline);
            if (infraTargetResult.IsError)
                return infraTargetResult.Errors;

            infraTarget = infraTargetResult.Value;
            if (!string.Equals(infraTarget.Alias, infraPushTarget.Alias, StringComparison.Ordinal))
                return Errors.GitRouting.AliasNotFound(infraPushTarget.Alias);
        }

        ResolvedRepositoryTarget? appTarget = null;
        if (codePushTarget is not null)
        {
            var appTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.ApplicationPipeline);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;

            appTarget = appTargetResult.Value;
            if (!string.Equals(appTarget.Alias, codePushTarget.Alias, StringComparison.Ordinal))
                return Errors.GitRouting.AliasNotFound(codePushTarget.Alias);
        }

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;
        var token = secretResult.Value;

        // Load latest pipeline blobs (split into infra/app buckets by sub-prefix).
        var pipelineSplitResult = await LoadLatestPipelineFilesSplitAsync(command.ProjectId.Value, cancellationToken);
        if (pipelineSplitResult.IsError)
            return pipelineSplitResult.Errors;
        var (infraPipelineFiles, appPipelineFiles) = pipelineSplitResult.Value;

        IReadOnlyDictionary<string, string>? bicepFiles = null;
        IReadOnlyDictionary<string, string>? bootstrapFiles = null;

        if (infraPushTarget is not null)
        {
            var bicepFilesResult = await LoadLatestArtifactFilesAsync(
                "bicep", command.ProjectId.Value, Errors.Project.BicepFilesNotFoundError, cancellationToken);
            if (bicepFilesResult.IsError)
                return bicepFilesResult.Errors;

            var bootstrapFilesResult = await LoadLatestArtifactFilesAsync(
                "bootstrap", command.ProjectId.Value, Errors.Project.BootstrapFilesNotFoundError, cancellationToken);
            if (bootstrapFilesResult.IsError)
                return bootstrapFilesResult.Errors;

            bicepFiles = bicepFilesResult.Value;
            bootstrapFiles = bootstrapFilesResult.Value;
        }

        var results = new List<RepoPushResult>((infraPushTarget is not null ? 1 : 0) + (codePushTarget is not null ? 1 : 0));

        if (infraPushTarget is not null)
        {
            // Push infra repository (Bicep + infra pipeline + bootstrap).
            var infraPushRequest = BuildPushRequest(
                token,
                infraTarget!,
                infraPushTarget,
                scopes:
                [
                    (infraTarget.BasePath, bicepFiles!),
                    (infraTarget.PipelineBasePath, infraPipelineFiles),
                    (infraTarget.PipelineBasePath, bootstrapFiles!),
                ]);

            results.Add(await PushOneAsync(infraTarget!, infraPushTarget.Alias, infraPushRequest, cancellationToken));
        }

        if (codePushTarget is not null)
        {
            // Push app repository (app pipeline files only).
            if (appPipelineFiles.Count == 0)
            {
                results.Add(new RepoPushResult(
                    Alias: codePushTarget.Alias,
                    Success: true,
                    BranchUrl: null,
                    CommitSha: null,
                    FileCount: 0,
                    ErrorCode: null,
                    ErrorDescription: "No application pipeline files to push."));
            }
            else
            {
                var appPushRequest = BuildPushRequest(
                    token,
                    appTarget!,
                    codePushTarget,
                    scopes: [(appTarget.PipelineBasePath, appPipelineFiles)]);

                results.Add(await PushOneAsync(appTarget!, codePushTarget.Alias, appPushRequest, cancellationToken));
            }
        }

        return new PushProjectArtifactsToMultiRepoResult(results);
    }

    private async Task<RepoPushResult> PushOneAsync(
        ResolvedRepositoryTarget target,
        string alias,
        ErrorOr<MultiScopeGitPushRequest> requestResult,
        CancellationToken cancellationToken)
    {
        if (requestResult.IsError)
        {
            var first = requestResult.Errors[0];
            return new RepoPushResult(alias, Success: false, BranchUrl: null, CommitSha: null,
                FileCount: 0, ErrorCode: first.Code, ErrorDescription: first.Description);
        }

        try
        {
            var provider = gitProviderFactory.Create(target.ProviderType);
            if (provider is not IGitMultiScopePushProviderService multiScopeProvider)
            {
                var error = Errors.GitRepository.PushFailed(
                    "The selected Git provider does not support multi-scope pushes.");
                return new RepoPushResult(alias, Success: false, BranchUrl: null, CommitSha: null,
                    FileCount: 0, ErrorCode: error.Code, ErrorDescription: error.Description);
            }

            var pushResult = await multiScopeProvider.PushScopedFilesAsync(requestResult.Value, cancellationToken);
            if (pushResult.IsError)
            {
                var first = pushResult.Errors[0];
                return new RepoPushResult(alias, Success: false, BranchUrl: null, CommitSha: null,
                    FileCount: 0, ErrorCode: first.Code, ErrorDescription: first.Description);
            }

            var value = pushResult.Value;
            return new RepoPushResult(alias, Success: true, BranchUrl: value.BranchUrl,
                CommitSha: value.CommitSha, FileCount: value.FileCount, ErrorCode: null, ErrorDescription: null);
        }
        catch (Exception ex)
        {
            return new RepoPushResult(alias, Success: false, BranchUrl: null, CommitSha: null,
                FileCount: 0, ErrorCode: "GitProvider.UnexpectedError", ErrorDescription: ex.Message);
        }
    }

    private async Task<ErrorOr<(IReadOnlyDictionary<string, string> Infra, IReadOnlyDictionary<string, string> App)>>
        LoadLatestPipelineFilesSplitAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var prefix = $"pipeline/project/{projectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.PipelineFilesNotFoundError(projectId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        var infra = new Dictionary<string, string>(StringComparer.Ordinal);
        var app = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var blobName in latestBlobs)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is null)
                continue;

            var relativePath = blobName[(latestPrefix.Length + 1)..];

            if (relativePath.StartsWith($"{InfraBucket}/", StringComparison.Ordinal))
                infra[relativePath[(InfraBucket.Length + 1)..]] = content;
            else if (relativePath.StartsWith($"{AppBucket}/", StringComparison.Ordinal))
                app[relativePath[(AppBucket.Length + 1)..]] = content;
            else
                infra[relativePath] = content; // legacy layout fallback (no bucket prefix)
        }

        if (infra.Count == 0 && app.Count == 0)
            return Errors.Project.PipelineFilesNotFoundError(projectId);

        var normalizedInfra = GeneratedPipelinePathNormalizer.Normalize(infra);
        var normalizedApp = GeneratedPipelinePathNormalizer.Normalize(app);

        return ((IReadOnlyDictionary<string, string>)normalizedInfra,
                (IReadOnlyDictionary<string, string>)normalizedApp);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestArtifactFilesAsync(
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

        if (files.Count == 0)
            return notFoundErrorFactory(projectId);

        return files;
    }

    private static ErrorOr<MultiScopeGitPushRequest> BuildPushRequest(
        string token,
        ResolvedRepositoryTarget target,
        RepoPushTarget pushTarget,
        IReadOnlyList<(string? BasePath, IReadOnlyDictionary<string, string> Files)> scopes)
    {
        var mergedScopes = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        foreach (var (basePath, files) in scopes)
        {
            var normalizedBasePath = NormalizeBasePath(basePath);
            if (!mergedScopes.TryGetValue(normalizedBasePath, out var scopedFiles))
            {
                scopedFiles = new Dictionary<string, string>(StringComparer.Ordinal);
                mergedScopes[normalizedBasePath] = scopedFiles;
            }

            foreach (var (relativePath, content) in files)
            {
                if (scopedFiles.TryGetValue(relativePath, out var existing)
                    && !string.Equals(existing, content, StringComparison.Ordinal))
                {
                    return Errors.GitRepository.PushFailed(
                        $"Generated file collision detected for path '{CombinePath(normalizedBasePath, relativePath)}'.");
                }

                scopedFiles[relativePath] = content;
            }
        }

        return new MultiScopeGitPushRequest
        {
            Token = token,
            Owner = target.Owner,
            RepositoryName = target.RepositoryName,
            BaseBranch = target.Branch,
            TargetBranchName = pushTarget.BranchName,
            CommitMessage = pushTarget.CommitMessage,
            Scopes = mergedScopes
                .Select(scope => new MultiScopeGitPushRequest.GitPushScope
                {
                    BasePath = string.IsNullOrEmpty(scope.Key) ? null : scope.Key,
                    Files = scope.Value,
                })
                .ToList(),
        };
    }

    private static string NormalizeBasePath(string? basePath) =>
        string.IsNullOrWhiteSpace(basePath) ? string.Empty : basePath.Trim('/');

    private static string CombinePath(string basePath, string relativePath) =>
        string.IsNullOrEmpty(basePath)
            ? relativePath.TrimStart('/')
            : $"{basePath}/{relativePath.TrimStart('/')}";
}
