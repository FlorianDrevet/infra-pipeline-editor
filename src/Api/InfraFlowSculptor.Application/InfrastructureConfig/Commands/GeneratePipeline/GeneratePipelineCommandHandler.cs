using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.PipelineGeneration;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Handles the <see cref="GeneratePipelineCommand"/>.</summary>
public sealed class GeneratePipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IProjectRepository projectRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    IConfigPipelineGenerationService configPipelineGenerationService,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver,
    IInfraConfigAccessService accessService)
    : ICommandHandler<GeneratePipelineCommand, GeneratePipelineResult>
{
    public async Task<ErrorOr<GeneratePipelineResult>> Handle(
        GeneratePipelineCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        var accessResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var domainConfig = accessResult.Value;

        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        // Load the enriched project snapshot needed for routing and project-level pipeline settings.
        var project = await projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken);

        // Resolve the pipeline-kind target to derive the BicepBasePath used by release pipeline YAML
        // (infra path inside the target repo). A missing repository is tolerated here: BicepBasePath
        // simply becomes null, matching the previous behavior when no Git configuration existed.
        string? bicepBasePath = null;
        if (project is not null && domainConfig is not null)
        {
            var targetResult = targetResolver.Resolve(project, domainConfig, ArtifactKind.Pipeline);
            if (!targetResult.IsError)
            {
                bicepBasePath = targetResult.Value.BasePath;
            }
        }

        var generationRequest = configPipelineGenerationService.BuildGenerationRequestForPipeline(
            config,
            project?.PipelineVariableGroups ?? [],
            project?.AgentPoolName,
            bicepBasePath);

        var result = pipelineGenerationEngine.Generate(generationRequest, config.Name);

        // ─── App Pipeline Generation ────────────────────────────────────────
        var appResult = await configPipelineGenerationService.GenerateAppPipelinesAsync(
                config,
                generationRequest,
                isMonoRepo: false,
                cancellationToken)
            .ConfigureAwait(false);

        // ─── Upload all artifacts ───────────────────────────────────────────
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var fileUris = new Dictionary<string, Uri>();

        foreach (var (path, content) in result.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "pipeline", command.InfrastructureConfigId, timestamp, path, content);
            fileUris[path] = uri;
        }

        foreach (var (path, content) in appResult.Files)
        {
            var uri = await artifactService.UploadArtifactAsync(
                "pipeline", command.InfrastructureConfigId, timestamp, path, content);
            fileUris[path] = uri;
        }

        // Upload shared app pipeline templates (only when there are app pipelines to reference them)
        if (appResult.Files.Count > 0)
        {
            foreach (var (path, content) in AppPipelineGenerationEngine.GenerateSharedTemplates())
            {
                var uri = await artifactService.UploadArtifactAsync(
                    "pipeline", command.InfrastructureConfigId, timestamp, path, content);
                fileUris[path] = uri;
            }
        }

        return new GeneratePipelineResult(fileUris);
    }
}
