using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
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
    IKeyVaultSecretClient keyVaultSecretClient,
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
        CancellationToken cancellationToken)
    {
        var targetsResult = ResolveTargets(project, command);
        if (targetsResult.IsError)
            return targetsResult.Errors;

        var (infraTarget, appTarget) = targetsResult.Value;

        var pipelineSplitResult = await LoadLatestPipelineFilesSplitAsync(
            command.ProjectId.Value,
            cancellationToken)
            .ConfigureAwait(false);
        if (pipelineSplitResult.IsError)
            return pipelineSplitResult.Errors;

        var (infraPipelineFiles, appPipelineFiles) = pipelineSplitResult.Value;

        var infraArtifactsResult = await LoadInfraArtifactsAsync(command, cancellationToken)
            .ConfigureAwait(false);
        if (infraArtifactsResult.IsError)
            return infraArtifactsResult.Errors;

        var appArtifactsResult = await LoadAppArtifactsAsync(command, cancellationToken)
            .ConfigureAwait(false);
        if (appArtifactsResult.IsError)
            return appArtifactsResult.Errors;

        var results = new List<RepoPushResult>(
            (command.Infra is not null ? 1 : 0) + (command.Code is not null ? 1 : 0));

        if (command.Infra is not null)
        {
            results.Add(await PushInfraAsync(
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
            if (!string.Equals(infraTarget.RepositoryId, command.Infra.RepositoryId.Value.ToString(), StringComparison.Ordinal))
                return Errors.GitRouting.RepositoryRoleMismatch(command.Infra.RepositoryId, RepositoryContentKindsEnum.Infrastructure);
        }

        ResolvedRepositoryTarget? appTarget = null;
        if (command.Code is not null)
        {
            var appTargetResult = targetResolver.Resolve(project, config: null, ArtifactKind.ApplicationPipeline);
            if (appTargetResult.IsError)
                return appTargetResult.Errors;

            appTarget = appTargetResult.Value;
            if (!string.Equals(appTarget.RepositoryId, command.Code.RepositoryId.Value.ToString(), StringComparison.Ordinal))
                return Errors.GitRouting.RepositoryRoleMismatch(command.Code.RepositoryId, RepositoryContentKindsEnum.ApplicationCode);
        }

        return (infraTarget, appTarget);
    }

    private async Task<ErrorOr<InfraArtifacts>> LoadInfraArtifactsAsync(
        PushProjectArtifactsToMultiRepoCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Infra is null)
            return new InfraArtifacts(null, null);

        var bicepFilesResult = await LoadLatestArtifactFilesAsync(
                BicepArtifactType,
                command.ProjectId.Value,
                Errors.Project.BicepFilesNotFoundError,
                cancellationToken)
            .ConfigureAwait(false);
        if (bicepFilesResult.IsError)
            return bicepFilesResult.Errors;

        var bootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
                command.ProjectId.Value,
            InfraBootstrapBucketPrefix,
            cancellationToken)
            .ConfigureAwait(false);
        if (bootstrapFilesResult.IsError)
            return bootstrapFilesResult.Errors;

        return new InfraArtifacts(bicepFilesResult.Value, bootstrapFilesResult.Value);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadAppArtifactsAsync(
        PushProjectArtifactsToMultiRepoCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Code is null)
        {
            return new Dictionary<string, string>();
        }

        var appBootstrapFilesResult = await LoadLatestBootstrapFilesAsync(
                command.ProjectId.Value,
            AppBootstrapBucketPrefix,
            cancellationToken)
            .ConfigureAwait(false);
        if (appBootstrapFilesResult.IsError)
            return appBootstrapFilesResult.Errors;

        return appBootstrapFilesResult.Value.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private async Task<RepoPushResult> PushInfraAsync(
        ResolvedRepositoryTarget infraTarget,
        RepoPushTarget infraPushTarget,
        IReadOnlyDictionary<string, string> bicepFiles,
        IReadOnlyDictionary<string, string> infraPipelineFiles,
        IReadOnlyDictionary<string, string> bootstrapFiles,
        CancellationToken cancellationToken)
    {
        var tokenResult = await GetPersonalAccessTokenAsync(infraTarget, cancellationToken).ConfigureAwait(false);
        if (tokenResult.IsError)
            return BuildFailedResult(infraPushTarget.RepositoryId, tokenResult.Errors[0]);

        var infraPushRequest = MultiScopeGitPushRequestBuilder.Build(
            token: tokenResult.Value,
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

        return await PushOneAsync(infraTarget, infraPushTarget.RepositoryId, infraPushRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<RepoPushResult> PushAppAsync(
        ResolvedRepositoryTarget appTarget,
        RepoPushTarget codePushTarget,
        IReadOnlyDictionary<string, string> appPipelineFiles,
        IReadOnlyDictionary<string, string> appBootstrapFiles,
        CancellationToken cancellationToken)
    {
        if (appPipelineFiles.Count == 0 && appBootstrapFiles.Count == 0)
        {
            return new RepoPushResult(
                RepositoryId: codePushTarget.RepositoryId,
                Success: true,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                ErrorCode: null,
                ErrorDescription: NoApplicationFilesToPushMessage);
        }

        var tokenResult = await GetPersonalAccessTokenAsync(appTarget, cancellationToken).ConfigureAwait(false);
        if (tokenResult.IsError)
            return BuildFailedResult(codePushTarget.RepositoryId, tokenResult.Errors[0]);

        var appPushRequest = MultiScopeGitPushRequestBuilder.Build(
            token: tokenResult.Value,
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

        return await PushOneAsync(appTarget, codePushTarget.RepositoryId, appPushRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<ErrorOr<string>> GetPersonalAccessTokenAsync(
        ResolvedRepositoryTarget target,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(target.PatSecretName))
            return Errors.GitRepository.SecretRetrievalFailed();

        return await keyVaultSecretClient.GetSecretAsync(target.PatSecretName, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<RepoPushResult> PushOneAsync(
        ResolvedRepositoryTarget target,
        ProjectRepositoryId repositoryId,
        ErrorOr<MultiScopeGitPushRequest> requestResult,
        CancellationToken cancellationToken)
    {
        if (requestResult.IsError)
        {
            return BuildFailedResult(repositoryId, requestResult.Errors[0]);
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
                return BuildFailedResult(repositoryId, first);
            }

            var value = pushResult.Value;
            return new RepoPushResult(
                repositoryId,
                Success: true,
                BranchUrl: value.BranchUrl,
                CommitSha: value.CommitSha,
                FileCount: value.FileCount,
                ErrorCode: null,
                ErrorDescription: null);
        }
            catch (OperationCanceledException)
            {
                throw;
            }
        catch (Exception ex)
        {
            return new RepoPushResult(
                repositoryId,
                Success: false,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                ErrorCode: UnexpectedGitProviderErrorCode,
                ErrorDescription: ex.Message);
        }
    }

    private static RepoPushResult BuildFailedResult(ProjectRepositoryId repositoryId, Error error)
    {
        return new RepoPushResult(
            repositoryId,
            Success: false,
            BranchUrl: null,
            CommitSha: null,
            FileCount: 0,
            ErrorCode: error.Code,
            ErrorDescription: error.Description);
    }

    private async Task<ErrorOr<(IReadOnlyDictionary<string, string> Infra, IReadOnlyDictionary<string, string> App)>>
        LoadLatestPipelineFilesSplitAsync(Guid projectId, CancellationToken cancellationToken)
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
                    SecondPostProcess: GeneratedPipelinePathNormalizer.Normalize),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestArtifactFilesAsync(
        string artifactType,
        Guid projectId,
        Func<Guid, Error> notFoundErrorFactory,
        CancellationToken cancellationToken)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
                blobService,
                blobPrefix: $"{artifactType}/project/{projectId}/",
                prefixSegmentCount: 4,
                notFoundErrorFactory,
                entityId: projectId,
                options: new BlobDownloadHelper.LatestBlobFilesOptions(
                    CancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestBootstrapFilesAsync(
        Guid projectId,
        string? bucketPrefix,
        CancellationToken cancellationToken)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
                blobService,
                blobPrefix: $"bootstrap/project/{projectId}/",
                prefixSegmentCount: 4,
                notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
                entityId: projectId,
                options: new BlobDownloadHelper.LatestBlobFilesOptions(
                    SubPrefix: bucketPrefix,
                    PostProcess: PrefixBootstrapPaths,
                    CancellationToken: cancellationToken))
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
