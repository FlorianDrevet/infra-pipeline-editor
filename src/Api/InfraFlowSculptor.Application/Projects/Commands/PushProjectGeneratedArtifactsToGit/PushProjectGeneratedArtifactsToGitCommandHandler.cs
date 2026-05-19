using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;

/// <summary>
/// Handles the <see cref="PushProjectGeneratedArtifactsToGitCommand"/>.
/// V2 routing via <see cref="IRepositoryTargetResolver"/>:
/// <list type="bullet">
/// <item><description>Resolves Infrastructure and Pipeline targets at the project level.</description></item>
/// <item><description>If the project declares a heterogeneous multi-repo topology (more than one
/// <c>ProjectRepository</c>), this single-commit mono-repo operation is rejected with
/// <see cref="Errors.GitRouting.AmbiguousProjectLevelGeneration"/>. Consumers must fall back to
/// the per-config push endpoints for such projects (V2-lite).</description></item>
/// </list>
/// </summary>
public sealed class PushProjectGeneratedArtifactsToGitCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IKeyVaultSecretClient keyVaultSecretClient,
    IMultiScopeGitPushExecutor multiScopeGitPushExecutor,
    IBlobService blobService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushProjectGeneratedArtifactsToGitCommand, PushBicepToGitResult>
{
    private const string BicepArtifactType = "bicep";
    private const string PipelineArtifactType = "pipeline";
    private const string BootstrapArtifactType = "bootstrap";
    private const int ProjectArtifactPrefixSegmentCount = 4;
    private const string UnsupportedMultiScopePushReason =
        "The selected Git provider does not support pushing multiple generated artifact roots in a single commit.";

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

        // V2-lite ambiguity gate: mono-repo single-commit push is only defined for a single target repository.
        // A project declaring multiple repository roles is a heterogeneous multi-repo topology and
        // must use the per-config push endpoints (Bicep/Pipeline/Bootstrap) instead.
        if (project.Repositories.Count > 1)
            return Errors.GitRouting.AmbiguousProjectLevelGeneration;

        // Resolve the project-level target for the pipeline kind — this populates both
        // BasePath (used by the Bicep scope) and PipelineBasePath (used by pipeline + bootstrap scopes).
        var targetResult = targetResolver.Resolve(project, config: null, ArtifactKind.Pipeline);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var secretResult = await keyVaultSecretClient.GetSecretAsync(
            target.PatSecretName ?? $"git-pat-{project.Id.Value}",
            cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var bicepFilesResult = await LoadLatestArtifactFilesAsync(
            BicepArtifactType,
            command.ProjectId.Value,
            Errors.Project.BicepFilesNotFoundError);
        if (bicepFilesResult.IsError)
            return bicepFilesResult.Errors;

        var pipelineFilesResult = await LoadLatestPipelineFilesAsync(command.ProjectId.Value);
        if (pipelineFilesResult.IsError)
            return pipelineFilesResult.Errors;

        var bootstrapFilesResult = await LoadLatestArtifactFilesAsync(
            BootstrapArtifactType,
            command.ProjectId.Value,
            Errors.Project.BootstrapFilesNotFoundError);
        if (bootstrapFilesResult.IsError)
            return bootstrapFilesResult.Errors;

        var multiScopePushRequest = MultiScopeGitPushRequestBuilder.Build(
            token: secretResult.Value,
            owner: target.Owner,
            repositoryName: target.RepositoryName,
            baseBranch: target.Branch,
            targetBranchName: command.BranchName,
            commitMessage: command.CommitMessage,
            scopes:
            [
                (target.BasePath, bicepFilesResult.Value),
                (target.PipelineBasePath, pipelineFilesResult.Value),
                (target.PipelineBasePath, bootstrapFilesResult.Value),
            ]);
        if (multiScopePushRequest.IsError)
            return multiScopePushRequest.Errors;

        return await multiScopeGitPushExecutor.PushAsync(
            target,
            multiScopePushRequest.Value,
            UnsupportedMultiScopePushReason,
            cancellationToken);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestArtifactFilesAsync(
        string artifactType,
        Guid projectId,
        Func<Guid, Error> notFoundErrorFactory)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"{artifactType}/project/{projectId}/",
            prefixSegmentCount: ProjectArtifactPrefixSegmentCount,
            notFoundErrorFactory,
            entityId: projectId);
    }

    private async Task<ErrorOr<IReadOnlyDictionary<string, string>>> LoadLatestPipelineFilesAsync(Guid projectId)
    {
        return await BlobDownloadHelper.GetLatestBlobFilesAsync(
            blobService,
            blobPrefix: $"{PipelineArtifactType}/project/{projectId}/",
            prefixSegmentCount: ProjectArtifactPrefixSegmentCount,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId: projectId,
            postProcess: GeneratedPipelinePathNormalizer.Normalize);
    }

}
