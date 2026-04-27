using ErrorOr;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;

/// <summary>
/// Handles the <see cref="PushPipelineToGitCommand"/>.
/// Uses <see cref="IRepositoryTargetResolver"/> (V2 routing) with <see cref="ArtifactKind.Pipeline"/>
/// to resolve the target repository from the configuration's <c>RepositoryBinding</c>,
/// falling back to the legacy project-level <c>GitRepositoryConfiguration</c> via the resolver's
/// built-in compatibility path.
/// </summary>
public sealed class PushPipelineToGitCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepo,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver)
    : ICommandHandler<PushPipelineToGitCommand, PushBicepToGitResult>
{
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushPipelineToGitCommand command, CancellationToken cancellationToken)
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

        var targetResult = targetResolver.Resolve(project, config, ArtifactKind.Pipeline);
        if (targetResult.IsError)
            return targetResult.Errors;

        var target = targetResult.Value;

        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var files = await artifactService.GetLatestFilesAsync(
            "pipeline", command.InfrastructureConfigId, cancellationToken);

        if (files is null || files.Count == 0)
            return Errors.InfrastructureConfig.PipelineFilesNotFoundError(command.InfrastructureConfigId);

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
            Files = GeneratedPipelinePathNormalizer.Normalize(files),
        }, cancellationToken);
    }
}
