using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using MediatR;
using AppPipelineMode = InfraFlowSculptor.GenerationCore.Models.AppPipelineMode;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Handles the <see cref="GeneratePipelineCommand"/>.</summary>
public sealed class GeneratePipelineCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    IProjectRepository projectRepository,
    PipelineGenerationEngine pipelineGenerationEngine,
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IAppPipelineRequestFactory appPipelineRequestFactory,
    IEnumerable<IResourceTypeBicepSpecGenerator> bicepGenerators,
    IGeneratedArtifactService artifactService,
    IRepositoryTargetResolver targetResolver,
    IInfrastructureConfigRepository infraConfigRepository,
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

        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        // Load project-level pipeline variable groups
        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
            new ProjectId(config.ProjectId), cancellationToken);
        // Load project (with Repositories + legacy GitRepositoryConfiguration) for V2 routing.
        var projectWithGit = await projectRepository.GetByIdWithAllAsync(
            new ProjectId(config.ProjectId), cancellationToken);

        // Load the domain InfrastructureConfig entity so the resolver can honor its RepositoryBinding.
        var domainConfig = await infraConfigRepository.GetByIdAsync(
            new InfrastructureConfigId(command.InfrastructureConfigId), cancellationToken);

        // Resolve the pipeline-kind target to derive the BicepBasePath used by release pipeline YAML
        // (infra path inside the target repo). A missing repository is tolerated here: BicepBasePath
        // simply becomes null, matching the previous behavior when no Git configuration existed.
        string? bicepBasePath = null;
        if (projectWithGit is not null && domainConfig is not null)
        {
            var targetResult = targetResolver.Resolve(projectWithGit, domainConfig, ArtifactKind.Pipeline);
            if (!targetResult.IsError)
            {
                bicepBasePath = targetResult.Value.BasePath;
            }
        }

        var generationRequest = GenerationRequestBuilder.BuildForPipeline(
            config,
            project?.PipelineVariableGroups ?? [],
            project?.AgentPoolName,
            bicepBasePath,
            bicepGenerators);
        var environments = generationRequest.Environments;

        var result = pipelineGenerationEngine.Generate(generationRequest, config.Name);

        // ─── App Pipeline Generation ────────────────────────────────────────
        var computeTypes = new HashSet<string>
        {
            AzureResourceTypes.ArmTypes.ContainerAppType,
            AzureResourceTypes.ArmTypes.WebAppType,
            AzureResourceTypes.ArmTypes.FunctionAppType,
        };

        var computeResources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Where(r => computeTypes.Contains(r.ResourceType))
            .ToList();

        var appRequests = new List<AppPipelineGenerationRequest>();

        foreach (var resource in computeResources)
        {
            var resourceId = new AzureResourceId(resource.Id);
            var req = await appPipelineRequestFactory.CreateAsync(
                resourceId, resource.ResourceType, cancellationToken).ConfigureAwait(false);

            if (req is null)
                continue;

            req.ConfigName = config.Name;
            req.Environments = environments;
            req.PipelineVariableGroups = generationRequest.PipelineVariableGroups;
            req.AgentPoolName = generationRequest.AgentPoolName;

            appRequests.Add(req);
        }

        var appPipelineMode = Enum.TryParse<AppPipelineMode>(config.AppPipelineMode, out var parsedMode)
            ? parsedMode
            : AppPipelineMode.Isolated;

        var appResult = appPipelineGenerationEngine.GenerateAll(appRequests, appPipelineMode, config.Name);

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
