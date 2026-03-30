using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;

/// <summary>Handles the <see cref="PushPipelineToGitCommand"/>.</summary>
public sealed class PushPipelineToGitCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepo,
    IProjectRepository projectRepo,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    IGeneratedArtifactService artifactService)
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

        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfig = project.GitRepositoryConfiguration;

        var secretResult = await keyVaultClient.GetSecretAsync(
            $"git-pat-{project.Id.Value}", cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        var files = await artifactService.GetLatestFilesAsync(
            "pipeline", command.InfrastructureConfigId, cancellationToken);

        if (files is null || files.Count == 0)
            return Error.NotFound(
                "PushPipelineToGit.NoFiles",
                $"No generated pipeline files found for configuration '{command.InfrastructureConfigId}'.");

        var gitProvider = gitProviderFactory.Create(gitConfig.ProviderType);
        return await gitProvider.PushFilesAsync(new GitPushRequest
        {
            Token = secretResult.Value,
            Owner = gitConfig.Owner,
            RepositoryName = gitConfig.RepositoryName,
            BaseBranch = gitConfig.DefaultBranch,
            TargetBranchName = command.BranchName,
            CommitMessage = command.CommitMessage,
            BasePath = gitConfig.PipelineBasePath,
            Files = files,
        }, cancellationToken);
    }
}
