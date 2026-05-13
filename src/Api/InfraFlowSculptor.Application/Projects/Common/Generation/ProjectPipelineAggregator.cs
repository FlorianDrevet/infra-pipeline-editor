using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Common.Generation;

/// <summary>
/// Generates and assembles mono-repo pipeline artifacts for project-level pipeline generation.
/// </summary>
public sealed class ProjectPipelineAggregator(
    PipelineGenerationEngine pipelineGenerationEngine,
    IConfigPipelineGenerationService configPipelineGenerationService)
    : IProjectPipelineAggregator
{
    /// <inheritdoc />
    public async Task<ErrorOr<MonoRepoPipelineResult>> GenerateAsync(
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups,
        string? agentPoolName,
        string? bicepBasePath,
        string? pipelineBasePath,
        CancellationToken cancellationToken = default)
    {
        var perConfigResults = new Dictionary<string, PipelineGenerationResult>(StringComparer.Ordinal);
        var environments = new List<EnvironmentDefinition>();
        var environmentShortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            var generationRequest = configPipelineGenerationService.BuildGenerationRequestForPipeline(
                config,
                projectVariableGroups,
                agentPoolName,
                bicepBasePath);

            var infraPipelineResult = pipelineGenerationEngine.Generate(generationRequest, config.Name, isMonoRepo: true);
            if (infraPipelineResult.IsError)
                return infraPipelineResult.Errors;

            var appPipelineResult = await configPipelineGenerationService.GenerateAppPipelinesAsync(
                    config,
                    generationRequest,
                    isMonoRepo: true,
                    cancellationToken)
                .ConfigureAwait(false);
            if (appPipelineResult.IsError)
                return appPipelineResult.Errors;

            perConfigResults[config.Name] = MergeFiles(infraPipelineResult.Value, appPipelineResult.Value);

            foreach (var environment in generationRequest.Environments)
            {
                if (environmentShortNames.Add(environment.ShortName))
                    environments.Add(environment);
            }
        }

        return MonoRepoPipelineAssembler.Assemble(
            perConfigResults,
            environments,
            agentPoolName,
            bicepBasePath,
            pipelineBasePath);
    }

    private static PipelineGenerationResult MergeFiles(
        PipelineGenerationResult infraPipelineResult,
        AppPipelineGenerationResult appPipelineResult)
    {
        if (appPipelineResult.Files.Count == 0)
            return infraPipelineResult;

        var mergedFiles = new Dictionary<string, string>(infraPipelineResult.Files, StringComparer.Ordinal);
        foreach (var (path, content) in appPipelineResult.Files)
            mergedFiles[path] = content;

        return new PipelineGenerationResult { TemplateFiles = mergedFiles };
    }
}