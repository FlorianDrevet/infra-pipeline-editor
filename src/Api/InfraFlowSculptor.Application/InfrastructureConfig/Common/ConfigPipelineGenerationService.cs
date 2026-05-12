using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using AppPipelineMode = InfraFlowSculptor.GenerationCore.Models.AppPipelineMode;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

internal sealed class ConfigPipelineGenerationService(
    AppPipelineGenerationEngine appPipelineGenerationEngine,
    IAppPipelineRequestFactory appPipelineRequestFactory,
    IEnumerable<IResourceTypeBicepSpecGenerator> bicepGenerators)
    : IConfigPipelineGenerationService
{
    private static readonly HashSet<string> ComputeTypes =
    [
        AzureResourceTypes.ArmTypes.ContainerAppType,
        AzureResourceTypes.ArmTypes.WebAppType,
        AzureResourceTypes.ArmTypes.FunctionAppType,
    ];

    public GenerationRequest BuildGenerationRequestForPipeline(
        InfrastructureConfigReadModel config,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups,
        string? agentPoolName,
        string? bicepBasePath)
    {
        return GenerationRequestBuilder.BuildForPipeline(
            config,
            projectVariableGroups,
            agentPoolName,
            bicepBasePath,
            bicepGenerators);
    }

    public async Task<AppPipelineGenerationResult> GenerateAppPipelinesAsync(
        InfrastructureConfigReadModel config,
        GenerationRequest generationRequest,
        bool isMonoRepo,
        CancellationToken cancellationToken = default)
    {
        var computeResources = config.ResourceGroups
            .SelectMany(resourceGroup => resourceGroup.Resources)
            .Where(resource => ComputeTypes.Contains(resource.ResourceType))
            .ToList();

        var appRequests = new List<AppPipelineGenerationRequest>();

        foreach (var resource in computeResources)
        {
            var resourceId = new AzureResourceId(resource.Id);
            var request = await appPipelineRequestFactory.CreateAsync(
                    resourceId,
                    resource.ResourceType,
                    cancellationToken)
                .ConfigureAwait(false);

            if (request is null)
                continue;

            request.ConfigName = config.Name;
            request.Environments = generationRequest.Environments;
            request.PipelineVariableGroups = generationRequest.PipelineVariableGroups;
            request.AgentPoolName = generationRequest.AgentPoolName;
            request.IsMonoRepo = isMonoRepo;

            appRequests.Add(request);
        }

        var appPipelineMode = Enum.TryParse<AppPipelineMode>(config.AppPipelineMode, out var parsedMode)
            ? parsedMode
            : AppPipelineMode.Isolated;

        return appPipelineGenerationEngine.GenerateAll(appRequests, appPipelineMode, config.Name);
    }
}