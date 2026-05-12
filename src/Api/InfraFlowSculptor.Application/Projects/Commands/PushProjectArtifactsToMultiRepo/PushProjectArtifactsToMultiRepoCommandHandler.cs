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
/// Pushes infra artifacts (Bicep + infra pipeline + bootstrap) and/or app artifacts (app pipeline + app bootstrap) to
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
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.LayoutPreset.Value != LayoutPresetEnum.SplitInfraCode)
            return Errors.GitRouting.LayoutNotSupportedForMultiRepoPush;

        var targetsResult = ResolveTargets(project, command);
        if (targetsResult.IsError)
            return targetsResult.Errors;
        var (infraTarget, appTarget) = targetsResult.Value;

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;
        var token = secretResult.Value;

        var pipelineSplitResult = await LoadLatestPipelineFilesSplitAsync(command.ProjectId.Value);
        if (pipelineSplitResult.IsError)
            return pipelineSplitResult.Errors;
        var (infraPipelineFiles, appPipelineFiles) = pipelineSplitResult.Value;

        var infraArtifactsResult = await LoadInfraArtifactsAsync(command);
        if (infraArtifactsResult.IsError) return infraArtifactsResult.Errors;
        var infraArtifacts = infraArtifactsResult.Value;

        var appArtifactsResult = await LoadAppArtifactsAsync(command);
        if (appArtifactsResult.IsError) return appArtifactsResult.Errors;
        var appBootstrapFiles = appArtifactsResult.Value;

        var results = new List<RepoPushResult>(
            (command.Infra is not null ? 1 : 0) + (command.Code is not null ? 1 : 0));

        if (command.Infra is not null)
        {
            results.Add(await PushInfraAsync(
                token, infraTarget!, command.Infra,
                infraArtifacts.Bicep!, infraPipelineFiles, infraArtifacts.Bootstrap!, cancellationToken));
        }

        if (command.Code is not null)
        {
            results.Add(await PushAppAsync(
                token, appTarget!, command.Code, appPipelineFiles, appBootstrapFiles, cancellationToken));
        }

        return new PushProjectArtifactsToMultiRepoResult(results);
    }

    private ErrorOr<(ResolvedRepositoryTarget? Infra, ResolvedRepositoryTarget? App)> ResolveTargets(
        Domain.ProjectAggregate.Project project,
        PushProjectArtifactsToMultiRepoCommand command)
    {
        ResolvedRepositoryTarget? infraTarget = null;
        if (command.Infra is not null)
        {
            var infraTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Pipeline);
            if (infraTargetResult.IsError)
                return infraTargetResult.Errors;

            infraTarget = infraTargetResult.Value;
            if (!string.Equals(infraTarget.Alias, command.Infra.Alias, StringComparison.Ordinal))
                return Errors.GitRouting.AliasNotFound(command.Infra.Alias);
        }

        ResolvedRepositoryTarget? appTarget = null;
        if (command.Code is not null)
        {
            var appTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.ApplicationPipeline);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;

            appTarget = appTargetResult.Value;
            if (!string.Equals(appTarget.Alias, command.Code.Alias, StringComparison.Ordinal))
                return Errors.GitRouting.AliasNotFound(command.Code.Alias);
        }

        return (infraTarget, appTarget);
    }

    private async Task<ErrorOr<InfraArtifacts>>
        LoadInfraArtifactsAsync(PushProjectArtifactsToMultiRepoCommand command)
    {
        if (command.Infra is null)
            return new InfraArtifacts(null, null);

        var bicepFilesResult = await LoadLatestArtifactFilesAsync(
            "bicep", command.ProjectId.Value, Errors.Project.BicepFilesNotFoundError);
        if (bicepFilesResult.IsError)
            return bicepFilesResult.Errors;

        var bootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
            command.ProjectId.Value, bucketPrefix: "infra/");
        if (bootstrapFilesResult.IsError)
            return bootstrapFilesResult.Errors;

        return new InfraArtifacts(bicepFilesResult.Value, bootstrapFilesResult.Value);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadAppArtifactsAsync(
        PushProjectArtifactsToMultiRepoCommand command)
    {
        if (command.Code is null)
            return ErrorOrFactory.From<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>());

        var appBootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
            command.ProjectId.Value, bucketPrefix: "app/");
        if (appBootstrapFilesResult.IsError)
            return appBootstrapFilesResult.Errors;

        return ErrorOrFactory.From(appBootstrapFilesResult.Value);
    }

    private readonly record struct InfraArtifacts(
        IReadOnlyDictionary<string, string>? Bicep,
        IReadOnlyDictionary<string, string>? Bootstrap);

    private async Task<RepoPushResult> PushInfraAsync(
        string token,
        ResolvedRepositoryTarget infraTarget,
        RepoPushTarget infraPushTarget,
        IReadOnlyDictionary<string, string> bicepFiles,
        IReadOnlyDictionary<string, string> infraPipelineFiles,
        IReadOnlyDictionary<string, string> bootstrapFiles,
        CancellationToken cancellationToken)
    {
        var infraPushRequest = MultiScopeGitPushRequestBuilder.Build(
            token: token,
            owner: infraTarget.Owner,
            repositoryName: infraTarget.RepositoryName,
            baseBranch: infraTarget.Branch,
            targetBranchName: infraPushTarget.BranchName,
            commitMessage: infraPushTarget.CommitMessage,
            scopes:
            [
                (infraTarget.BasePath, bicepFiles),
                (infraTarget.PipelineBasePath, infraPipelineFiles),
                (infraTarget.PipelineBasePath, bootstrapFiles),
            ]);

        return await PushOneAsync(infraTarget, infraPushTarget.Alias, infraPushRequest, cancellationToken);
    }

    private async Task<RepoPushResult> PushAppAsync(
        string token,
        ResolvedRepositoryTarget appTarget,
        RepoPushTarget codePushTarget,
        IReadOnlyDictionary<string, string> appPipelineFiles,
        IReadOnlyDictionary<string, string> appBootstrapFiles,
        CancellationToken cancellationToken)
    {
        if (appPipelineFiles.Count == 0 && appBootstrapFiles.Count == 0)
        {
            return new RepoPushResult(
                Alias: codePushTarget.Alias,
                Success: true,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                ErrorCode: null,
                ErrorDescription: "No application pipeline files to push.");
        }

        var appPushRequest = MultiScopeGitPushRequestBuilder.Build(
            token: token,
            owner: appTarget.Owner,
            repositoryName: appTarget.RepositoryName,
            baseBranch: appTarget.Branch,
            targetBranchName: codePushTarget.BranchName,
            commitMessage: codePushTarget.CommitMessage,
            scopes:
            [
                (appTarget.PipelineBasePath, appPipelineFiles),
                (appTarget.PipelineBasePath, appBootstrapFiles),
            ]);

        return await PushOneAsync(appTarget, codePushTarget.Alias, appPushRequest, cancellationToken);
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
        LoadLatestPipelineFilesSplitAsync(Guid projectId)
    {
        return await BlobDownloadHelper.GetLatestDualBucketBlobFilesAsync(
            blobService,
            blobPrefix: $"pipeline/project/{projectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId: projectId,
            firstBucketName: InfraBucket,
            secondBucketName: AppBucket,
            legacyDefaultBucketName: InfraBucket,
            firstPostProcess: GeneratedPipelinePathNormalizer.Normalize,
            secondPostProcess: GeneratedPipelinePathNormalizer.Normalize);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestArtifactFilesAsync(
        string artifactType,
        Guid projectId,
        Func<Guid, Error> notFoundErrorFactory)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"{artifactType}/project/{projectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory,
            entityId: projectId);
    }

    /// <summary>
    /// Loads bootstrap pipeline files from blob storage, optionally filtering by bucket prefix.
    /// In <c>SplitInfraCode</c> layout, bootstrap blobs are stored under <c>infra/</c> and <c>app/</c> sub-prefixes.
    /// Passing a <paramref name="bucketPrefix"/> (e.g. <c>"infra/"</c>) returns only that bucket's files with the prefix stripped.
    /// </summary>
    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestBootstrapFilesAsync(
        Guid projectId, string? bucketPrefix)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"bootstrap/project/{projectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
            entityId: projectId,
            subPrefix: bucketPrefix,
            postProcess: PrefixBootstrapPaths);
    }

    private static IReadOnlyDictionary<string, string> PrefixBootstrapPaths(Dictionary<string, string> files)
    {
        return files.ToDictionary(
            static pair => $".azuredevops/{pair.Key}",
            static pair => pair.Value,
            StringComparer.Ordinal);
    }

}
