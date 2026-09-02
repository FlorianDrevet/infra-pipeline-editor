using ErrorOr;
using InfraFlowSculptor.Application.Common.Generation;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.Extensions.Logging;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>
/// Pushes generated artifacts for configuration-owned repositories in a MultiRepo project.
/// </summary>
public sealed class ProjectMultiRepoArtifactsPushService(
    IInfrastructureConfigRepository configRepository,
    IGeneratedArtifactService artifactService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IMultiScopeGitPushExecutor pushExecutor,
    IRepositoryTargetResolver targetResolver,
    ILogger<ProjectMultiRepoArtifactsPushService> logger)
    : IProjectMultiRepoArtifactsPushService
{
    private const string BicepArtifactType = "bicep";
    private const string PipelineArtifactType = "pipeline";
    private const string BootstrapArtifactType = "bootstrap";
    private const string InfraBootstrapPrefix = "infra/";
    private const string AppBootstrapPrefix = "app/";
    private const string UnsupportedMultiScopePushReason =
        "The selected Git provider does not support multi-scope pushes.";
    private const string UnexpectedGitProviderErrorCode = "GitProvider.UnexpectedError";
    private const string UnexpectedGitProviderErrorDescription = "The repository push could not be completed.";

    /// <inheritdoc />
    public async Task<ErrorOr<PushProjectMultiRepoArtifactsResult>> PushAsync(
        PushProjectMultiRepoArtifactsCommand command,
        Project project,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(project);

        var plansResult = await BuildPlansAsync(command, project, cancellationToken)
            .ConfigureAwait(false);
        if (plansResult.IsError)
            return plansResult.Errors;

        var results = new List<ConfigRepositoryPushResult>(plansResult.Value.Count);
        foreach (var plan in plansResult.Value)
        {
            results.Add(await ExecutePushAsync(plan, cancellationToken).ConfigureAwait(false));
        }

        return new PushProjectMultiRepoArtifactsResult(results);
    }

    private async Task<ErrorOr<IReadOnlyList<PushPlan>>> BuildPlansAsync(
        PushProjectMultiRepoArtifactsCommand command,
        Project project,
        CancellationToken cancellationToken)
    {
        var plans = new List<PushPlan>();
        foreach (var configurationTarget in command.Configurations.OrderBy(
                     target => target.InfrastructureConfigId.Value))
        {
            var configurationPlansResult = await BuildConfigurationPlansAsync(
                    configurationTarget,
                    project,
                    cancellationToken)
                .ConfigureAwait(false);
            if (configurationPlansResult.IsError)
                return configurationPlansResult.Errors;

            plans.AddRange(configurationPlansResult.Value);
        }

        return plans;
    }

    private async Task<ErrorOr<IReadOnlyList<PushPlan>>> BuildConfigurationPlansAsync(
        InfrastructureConfigPushTarget configurationTarget,
        Project project,
        CancellationToken cancellationToken)
    {
        var config = await configRepository
            .GetByIdAsync(configurationTarget.InfrastructureConfigId, cancellationToken)
            .ConfigureAwait(false);
        if (config is null || config.ProjectId != project.Id)
            return Errors.InfrastructureConfig.NotFoundError(configurationTarget.InfrastructureConfigId);

        if (config.LayoutMode is null)
            return Errors.InfraConfigRepository.LayoutModeRequired();

        var repositoryPlansResult = ResolveRepositoryPlans(
            project,
            config,
            configurationTarget.Repositories);
        if (repositoryPlansResult.IsError)
            return repositoryPlansResult.Errors;

        var includeInfrastructureArtifacts = repositoryPlansResult.Value.Any(plan =>
            plan.ArtifactKind == ArtifactKind.Infrastructure);
        var artifactsResult = await LoadGeneratedArtifactsAsync(
                configurationTarget.InfrastructureConfigId,
            config.LayoutMode.Value,
                includeInfrastructureArtifacts,
                cancellationToken)
            .ConfigureAwait(false);
        if (artifactsResult.IsError)
            return artifactsResult.Errors;

        return CreatePushPlans(
            configurationTarget.InfrastructureConfigId,
            config.LayoutMode.Value,
            repositoryPlansResult.Value,
            artifactsResult.Value);
    }

    private ErrorOr<IReadOnlyList<ResolvedRepositoryPlan>> ResolveRepositoryPlans(
        Project project,
        DomainInfrastructureConfig config,
        IReadOnlyList<ConfigRepositoryPushTarget> repositoryTargets)
    {
        var plans = new List<ResolvedRepositoryPlan>();
        foreach (var repositoryTarget in repositoryTargets.OrderBy(target => target.RepositoryId.Value))
        {
            var repository = config.Repositories.FirstOrDefault(
                candidate => candidate.Id == repositoryTarget.RepositoryId);
            if (repository is null)
                return Errors.InfraConfigRepository.NotFound(repositoryTarget.RepositoryId);

            var artifactKindResult = ResolveArtifactKind(config.LayoutMode!.Value, repository);
            if (artifactKindResult.IsError)
                return artifactKindResult.Errors;

            var resolvedTargetResult = targetResolver.Resolve(
                project,
                config,
                artifactKindResult.Value);
            if (resolvedTargetResult.IsError)
                return resolvedTargetResult.Errors;

            if (!string.Equals(
                    resolvedTargetResult.Value.RepositoryId,
                    repositoryTarget.RepositoryId.Value.ToString(),
                    StringComparison.Ordinal))
            {
                return Errors.InfraConfigRepository.NotFound(repositoryTarget.RepositoryId);
            }

            plans.Add(new ResolvedRepositoryPlan(
                repositoryTarget,
                resolvedTargetResult.Value,
                artifactKindResult.Value));
        }

        return plans;
    }

    private static ErrorOr<IReadOnlyList<PushPlan>> CreatePushPlans(
        InfrastructureConfigId configId,
        ConfigLayoutModeEnum layoutMode,
        IReadOnlyList<ResolvedRepositoryPlan> repositoryPlans,
        GeneratedArtifacts artifacts)
    {
        var plans = new List<PushPlan>(repositoryPlans.Count);
        foreach (var repositoryPlan in repositoryPlans)
        {
            var scopes = SelectScopes(layoutMode, repositoryPlan.ArtifactKind, artifacts);
            if (scopes.Count == 0)
                return Errors.InfrastructureConfig.PipelineFilesNotFoundError(configId.Value);

            var requestResult = BuildPushRequest(
                repositoryPlan.Target,
                repositoryPlan.RepositoryTarget,
                scopes,
                token: string.Empty);
            if (requestResult.IsError)
                return requestResult.Errors;

            plans.Add(new PushPlan(
                configId,
                repositoryPlan.RepositoryTarget,
                repositoryPlan.Target,
                requestResult.Value));
        }

        return plans;
    }

    private static IReadOnlyList<PushScope> SelectScopes(
        ConfigLayoutModeEnum layoutMode,
        ArtifactKind artifactKind,
        GeneratedArtifacts artifacts)
    {
        if (layoutMode == ConfigLayoutModeEnum.AllInOne)
        {
            return RemoveEmptyScopes(
            [
                new PushScope(artifacts.BicepFiles, BasePathKind.Infrastructure),
                new PushScope(artifacts.InfraPipelineFiles, BasePathKind.Pipeline),
                new PushScope(artifacts.AppPipelineFiles, BasePathKind.Pipeline),
                new PushScope(artifacts.InfraBootstrapFiles, BasePathKind.Pipeline),
                new PushScope(artifacts.AppBootstrapFiles, BasePathKind.Pipeline),
            ]);
        }

        if (artifactKind == ArtifactKind.Infrastructure)
        {
            return RemoveEmptyScopes(
            [
                new PushScope(artifacts.BicepFiles, BasePathKind.Infrastructure),
                new PushScope(artifacts.InfraPipelineFiles, BasePathKind.Pipeline),
                new PushScope(artifacts.InfraBootstrapFiles, BasePathKind.Pipeline),
            ]);
        }

        return RemoveEmptyScopes(
        [
            new PushScope(artifacts.AppPipelineFiles, BasePathKind.Pipeline),
            new PushScope(artifacts.AppBootstrapFiles, BasePathKind.Pipeline),
        ]);
    }

    private async Task<ErrorOr<GeneratedArtifacts>> LoadGeneratedArtifactsAsync(
        InfrastructureConfigId configId,
        ConfigLayoutModeEnum layoutMode,
        bool includeBicep,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, string> bicepFiles = new Dictionary<string, string>();
        if (includeBicep)
        {
            var bicepFilesResult = await artifactService
                .GetLatestFilesAsync(BicepArtifactType, configId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (bicepFilesResult is null || bicepFilesResult.Count == 0)
                return Errors.InfrastructureConfig.BicepFilesNotFoundError(configId.Value);

            bicepFiles = bicepFilesResult;
        }

        var pipelineFiles = await artifactService
            .GetLatestFilesAsync(PipelineArtifactType, configId.Value, cancellationToken)
            .ConfigureAwait(false);
        if (pipelineFiles is null || pipelineFiles.Count == 0)
            return Errors.InfrastructureConfig.PipelineFilesNotFoundError(configId.Value);

        var bootstrapFiles = await artifactService
            .GetLatestFilesAsync(BootstrapArtifactType, configId.Value, cancellationToken)
            .ConfigureAwait(false);
        if (bootstrapFiles is null || bootstrapFiles.Count == 0)
            return Errors.InfrastructureConfig.BootstrapFilesNotFoundError(configId.Value);

        var pipelineFilesByRole = AppPipelineFileClassifier.Split(
            GeneratedPipelinePathNormalizer.Normalize(pipelineFiles));
        var bootstrapFilesByRoleResult = SplitBootstrapFiles(
            bootstrapFiles,
            layoutMode,
            configId.Value);
        if (bootstrapFilesByRoleResult.IsError)
            return bootstrapFilesByRoleResult.Errors;

        return new GeneratedArtifacts(
            bicepFiles,
            pipelineFilesByRole.Infra,
            pipelineFilesByRole.App,
            bootstrapFilesByRoleResult.Value.Infra,
            bootstrapFilesByRoleResult.Value.App);
    }

    private async Task<ConfigRepositoryPushResult> ExecutePushAsync(
        PushPlan plan,
        CancellationToken cancellationToken)
    {
        var tokenResult = await GetPersonalAccessTokenAsync(plan.Target, cancellationToken)
            .ConfigureAwait(false);
        if (tokenResult.IsError)
            return BuildFailedResult(plan, tokenResult.FirstError);

        var request = new MultiScopeGitPushRequest
        {
            Token = tokenResult.Value,
            Owner = plan.Request.Owner,
            RepositoryName = plan.Request.RepositoryName,
            BaseBranch = plan.Request.BaseBranch,
            TargetBranchName = plan.Request.TargetBranchName,
            CommitMessage = plan.Request.CommitMessage,
            Scopes = plan.Request.Scopes,
        };

        try
        {
            var pushResult = await pushExecutor
                .PushAsync(
                    plan.Target,
                    request,
                    UnsupportedMultiScopePushReason,
                    cancellationToken)
                .ConfigureAwait(false);
            if (pushResult.IsError)
                return BuildFailedResult(plan, pushResult.FirstError);

            return new ConfigRepositoryPushResult(
                plan.InfrastructureConfigId,
                plan.RepositoryTarget.RepositoryId,
                Success: true,
                pushResult.Value.BranchUrl,
                pushResult.Value.CommitSha,
                pushResult.Value.FileCount,
                ErrorCode: null,
                ErrorDescription: null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unexpected error pushing generated artifacts to configuration repository {RepositoryId}.",
                plan.RepositoryTarget.RepositoryId.Value);

            return new ConfigRepositoryPushResult(
                plan.InfrastructureConfigId,
                plan.RepositoryTarget.RepositoryId,
                Success: false,
                BranchUrl: null,
                CommitSha: null,
                FileCount: 0,
                UnexpectedGitProviderErrorCode,
                UnexpectedGitProviderErrorDescription);
        }
    }

    private async Task<ErrorOr<string>> GetPersonalAccessTokenAsync(
        ResolvedRepositoryTarget target,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(target.PatSecretName))
            return Errors.GitRepository.SecretRetrievalFailed();

        return await keyVaultSecretClient
            .GetSecretAsync(target.PatSecretName, cancellationToken)
            .ConfigureAwait(false);
    }

    private static ConfigRepositoryPushResult BuildFailedResult(PushPlan plan, Error error) =>
        new(
            plan.InfrastructureConfigId,
            plan.RepositoryTarget.RepositoryId,
            Success: false,
            BranchUrl: null,
            CommitSha: null,
            FileCount: 0,
            error.Code,
            error.Description);

    private static ErrorOr<MultiScopeGitPushRequest> BuildPushRequest(
        ResolvedRepositoryTarget target,
        ConfigRepositoryPushTarget repositoryTarget,
        IReadOnlyList<PushScope> scopes,
        string token)
    {
        return MultiScopeGitPushRequestBuilder.Build(
            token,
            target.Owner,
            target.RepositoryName,
            target.Branch,
            repositoryTarget.BranchName,
            repositoryTarget.CommitMessage,
            scopes
                .Select(scope =>
                (
                    scope.BasePathKind == BasePathKind.Infrastructure
                        ? target.BasePath
                        : target.PipelineBasePath,
                    scope.Files))
                .ToList());
    }

    private static IReadOnlyList<PushScope> RemoveEmptyScopes(IEnumerable<PushScope> scopes)
    {
        return scopes
            .Where(scope => scope.Files.Count > 0)
            .ToList();
    }

    private static ErrorOr<ArtifactKind> ResolveArtifactKind(
        ConfigLayoutModeEnum layoutMode,
        InfraConfigRepository repository)
    {
        if (layoutMode == ConfigLayoutModeEnum.AllInOne)
        {
            return repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure)
                && repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode)
                ? ArtifactKind.Infrastructure
                : Errors.InfraConfigRepository.NotFound(repository.Id);
        }

        if (repository.ContentKinds.Has(RepositoryContentKindsEnum.Infrastructure))
            return ArtifactKind.Infrastructure;

        if (repository.ContentKinds.Has(RepositoryContentKindsEnum.ApplicationCode))
            return ArtifactKind.ApplicationPipeline;

        return Errors.InfraConfigRepository.NotFound(repository.Id);
    }

    private static ErrorOr<(IReadOnlyDictionary<string, string> Infra, IReadOnlyDictionary<string, string> App)>
        SplitBootstrapFiles(
            IReadOnlyDictionary<string, string> files,
            ConfigLayoutModeEnum layoutMode,
            Guid configId)
    {
        var infraFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var appFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        if (layoutMode == ConfigLayoutModeEnum.AllInOne)
            return (new Dictionary<string, string>(files, StringComparer.Ordinal), appFiles);

        foreach (var (path, content) in files)
        {
            if (path.StartsWith(InfraBootstrapPrefix, StringComparison.Ordinal))
            {
                infraFiles[path[InfraBootstrapPrefix.Length..]] = content;
                continue;
            }

            if (path.StartsWith(AppBootstrapPrefix, StringComparison.Ordinal))
            {
                appFiles[path[AppBootstrapPrefix.Length..]] = content;
                continue;
            }

            return Errors.InfrastructureConfig.BootstrapFilesNotFoundError(configId);
        }

        if (infraFiles.Count == 0 || appFiles.Count == 0)
            return Errors.InfrastructureConfig.BootstrapFilesNotFoundError(configId);

        return (infraFiles, appFiles);
    }

    private enum BasePathKind
    {
        Infrastructure,
        Pipeline,
    }

    private sealed record PushScope(
        IReadOnlyDictionary<string, string> Files,
        BasePathKind BasePathKind);

    private sealed record PushPlan(
        InfrastructureConfigId InfrastructureConfigId,
        ConfigRepositoryPushTarget RepositoryTarget,
        ResolvedRepositoryTarget Target,
        MultiScopeGitPushRequest Request);

    private sealed record ResolvedRepositoryPlan(
        ConfigRepositoryPushTarget RepositoryTarget,
        ResolvedRepositoryTarget Target,
        ArtifactKind ArtifactKind);

    private sealed record GeneratedArtifacts(
        IReadOnlyDictionary<string, string> BicepFiles,
        IReadOnlyDictionary<string, string> InfraPipelineFiles,
        IReadOnlyDictionary<string, string> AppPipelineFiles,
        IReadOnlyDictionary<string, string> InfraBootstrapFiles,
        IReadOnlyDictionary<string, string> AppBootstrapFiles);
}