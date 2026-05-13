using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>
/// Encapsulates the multi-repository artifact push workflow for split-layout projects.
/// </summary>
public sealed class MultiRepoProjectArtifactsPushService(
    IMultiScopeGitPushExecutor multiScopeGitPushExecutor,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : IMultiRepoProjectArtifactsPushService
{
    private const string InfraBucket = "infra";
    private const string AppBucket = "app";
    private const string BicepArtifactType = "bicep";
    private const string InfraBootstrapBucketPrefix = "infra/";
    private const string AppBootstrapBucketPrefix = "app/";
    private const string UnsupportedMultiScopePushReason =
        "The selected Git provider does not support multi-scope pushes.";
    private const string UnexpectedGitProviderErrorCode = "GitProvider.UnexpectedError";
    private const string NoApplicationFilesToPushMessage = "No application pipeline files to push.";

    /// <inheritdoc />
    public async Task<ErrorOr<PushProjectArtifactsToMultiRepoResult>> PushAsync(
        PushProjectArtifactsToMultiRepoCommand command,
        Project project,
        string token,
        CancellationToken cancellationToken)
    {
        var targetsResult = ResolveTargets(project, command);
        if (targetsResult.IsError)
            return targetsResult.Errors;

        var (infraTarget, appTarget) = targetsResult.Value;

        var pipelineSplitResult = await LoadLatestPipelineFilesSplitAsync(command.ProjectId.Value)
            .ConfigureAwait(false);
        if (pipelineSplitResult.IsError)
            return pipelineSplitResult.Errors;

        var (infraPipelineFiles, appPipelineFiles) = pipelineSplitResult.Value;

        var infraArtifactsResult = await LoadInfraArtifactsAsync(command).ConfigureAwait(false);
        if (infraArtifactsResult.IsError)
            return infraArtifactsResult.Errors;

        var appArtifactsResult = await LoadAppArtifactsAsync(command).ConfigureAwait(false);
        if (appArtifactsResult.IsError)
            return appArtifactsResult.Errors;

        var results = new List<RepoPushResult>(
            (command.Infra is not null ? 1 : 0) + (command.Code is not null ? 1 : 0));

        if (command.Infra is not null)
        {
            results.Add(await PushInfraAsync(
                    token,
                    infraTarget!,
                    command.Infra,
                    infraArtifactsResult.Value.Bicep!,
                    infraPipelineFiles,
                    infraArtifactsResult.Value.Bootstrap!,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        if (command.Code is not null)
        {
            results.Add(await PushAppAsync(
                    token,
                    appTarget!,
                    command.Code,
                    appPipelineFiles,
                    appArtifactsResult.Value,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        return new PushProjectArtifactsToMultiRepoResult(results);
    }

    private ErrorOr<(ResolvedRepositoryTarget? Infra, ResolvedRepositoryTarget? App)> ResolveTargets(
        Project project,
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

    private async Task<ErrorOr<InfraArtifacts>> LoadInfraArtifactsAsync(
        PushProjectArtifactsToMultiRepoCommand command)
    {
        if (command.Infra is null)
            return new InfraArtifacts(null, null);

        var bicepFilesResult = await LoadLatestArtifactFilesAsync(
                BicepArtifactType,
                command.ProjectId.Value,
                Errors.Project.BicepFilesNotFoundError)
            .ConfigureAwait(false);
        if (bicepFilesResult.IsError)
            return bicepFilesResult.Errors;

        var bootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
                command.ProjectId.Value,
                InfraBootstrapBucketPrefix)
            .ConfigureAwait(false);
        if (bootstrapFilesResult.IsError)
            return bootstrapFilesResult.Errors;

        return new InfraArtifacts(bicepFilesResult.Value, bootstrapFilesResult.Value);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadAppArtifactsAsync(
        PushProjectArtifactsToMultiRepoCommand command)
    {
        if (command.Code is null)
        {
            return ErrorOrFactory.From<IReadOnlyDictionary<string, string>>(
                new Dictionary<string, string>());
        }

        var appBootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
                command.ProjectId.Value,
                AppBootstrapBucketPrefix)
            .ConfigureAwait(false);
        if (appBootstrapFilesResult.IsError)
            return appBootstrapFilesResult.Errors;

        return ErrorOrFactory.From(appBootstrapFilesResult.Value);
    }

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

        return await PushOneAsync(infraTarget, infraPushTarget.Alias, infraPushRequest, cancellationToken)
            .ConfigureAwait(false);
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
                ErrorDescription: NoApplicationFilesToPushMessage);
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

        return await PushOneAsync(appTarget, codePushTarget.Alias, appPushRequest, cancellationToken)
            .ConfigureAwait(false);
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
            return new RepoPushResult(
                alias,
                Success: false,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                ErrorCode: first.Code,
                ErrorDescription: first.Description);
        }

        try
        {
            var pushResult = await multiScopeGitPushExecutor.PushAsync(
                    target,
                    requestResult.Value,
                    UnsupportedMultiScopePushReason,
                    cancellationToken)
                .ConfigureAwait(false);
            if (pushResult.IsError)
            {
                var first = pushResult.Errors[0];
                return new RepoPushResult(
                    alias,
                    Success: false,
                    BranchUrl: null,
                    CommitSha: null,
                    FileCount: 0,
                    ErrorCode: first.Code,
                    ErrorDescription: first.Description);
            }

            var value = pushResult.Value;
            return new RepoPushResult(
                alias,
                Success: true,
                BranchUrl: value.BranchUrl,
                CommitSha: value.CommitSha,
                FileCount: value.FileCount,
                ErrorCode: null,
                ErrorDescription: null);
        }
        catch (Exception ex)
        {
            return new RepoPushResult(
                alias,
                Success: false,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                ErrorCode: UnexpectedGitProviderErrorCode,
                ErrorDescription: ex.Message);
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
                new BlobDownloadHelper.DualBucketBlobFilesOptions(
                    FirstBucketName: InfraBucket,
                    SecondBucketName: AppBucket,
                    LegacyDefaultBucketName: InfraBucket,
                    FirstPostProcess: GeneratedPipelinePathNormalizer.Normalize,
                    SecondPostProcess: GeneratedPipelinePathNormalizer.Normalize))
            .ConfigureAwait(false);
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
                entityId: projectId)
            .ConfigureAwait(false);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestBootstrapFilesAsync(
        Guid projectId,
        string? bucketPrefix)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
                blobService,
                blobPrefix: $"bootstrap/project/{projectId}/",
                prefixSegmentCount: 4,
                notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
                entityId: projectId,
                subPrefix: bucketPrefix,
                postProcess: PrefixBootstrapPaths)
            .ConfigureAwait(false);
    }

    private static IReadOnlyDictionary<string, string> PrefixBootstrapPaths(Dictionary<string, string> files)
    {
        return files.ToDictionary(
            static pair => $".azuredevops/{pair.Key}",
            static pair => pair.Value,
            StringComparer.Ordinal);
    }

    private readonly record struct InfraArtifacts(
        IReadOnlyDictionary<string, string>? Bicep,
        IReadOnlyDictionary<string, string>? Bootstrap);
}