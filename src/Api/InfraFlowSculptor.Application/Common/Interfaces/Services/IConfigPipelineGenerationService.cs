using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Builds pipeline generation requests and generates application pipeline artifacts for a single configuration.
/// </summary>
public interface IConfigPipelineGenerationService
{
    /// <summary>
    /// Builds the infra pipeline generation request for one configuration.
    /// </summary>
    GenerationRequest BuildGenerationRequestForPipeline(
        InfrastructureConfigReadModel config,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups,
        string? agentPoolName,
        string? bicepBasePath);

    /// <summary>
    /// Generates application pipeline artifacts for the compute resources of one configuration.
    /// </summary>
    Task<ErrorOr<AppPipelineGenerationResult>> GenerateAppPipelinesAsync(
        InfrastructureConfigReadModel config,
        GenerationRequest generationRequest,
        bool isMonoRepo,
        CancellationToken cancellationToken = default);
}