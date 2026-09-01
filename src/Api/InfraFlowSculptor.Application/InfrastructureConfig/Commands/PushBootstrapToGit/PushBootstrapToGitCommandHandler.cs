using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;

/// <summary>
/// Handles the <see cref="PushBootstrapToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> (V2 routing) with <see cref="ArtifactKind.Bootstrap"/>
/// and a non-null configuration, resolving to the infrastructure configuration's own repository
/// (<c>MultiRepo</c> layout). Unlike <c>PushPipelineToGitCommandHandler</c>, generated files are
/// pushed as-is: <c>bootstrap.pipeline.yml</c> lives at the root of the pipeline base path, so
/// <c>GeneratedPipelinePathNormalizer</c> (which rewrites per-resource pipeline paths) must not be
/// applied here.
/// </summary>
public sealed class PushBootstrapToGitCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepo,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushBootstrapToGitCommand, PushBicepToGitResult>
{
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushBootstrapToGitCommand command, CancellationToken cancellationToken)
    {
        var infraConfigId = new InfrastructureConfigId(command.InfrastructureConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(infraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = await infraConfigRepo.GetByIdAsync(infraConfigId, cancellationToken);
        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var project = await projectRepo.GetByIdWithAllAsync(config.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(config.ProjectId);

        var targetResult = targetResolver.Resolve(project, config, ArtifactKind.Bootstrap);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var secretResult = await keyVaultClient.GetSecretAsync(
            target.PatSecretName ?? $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var files = await artifactService.GetLatestFilesAsync(
            "bootstrap", command.InfrastructureConfigId, cancellationToken);

        if (files is null || files.Count == 0)
            return Errors.InfrastructureConfig.BootstrapFilesNotFoundError(command.InfrastructureConfigId);

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
            Files = files,
        }, cancellationToken);
    }
}
