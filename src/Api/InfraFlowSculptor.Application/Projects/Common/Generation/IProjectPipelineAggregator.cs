using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Common.Generation;

/// <summary>
/// Generates and assembles mono-repo pipeline artifacts for all configurations of a project.
/// </summary>
public interface IProjectPipelineAggregator
{
    /// <summary>
    /// Generates per-configuration pipeline artifacts, merges application wrappers, deduplicates environments,
    /// and assembles the final mono-repo pipeline output.
    /// </summary>
    /// <param name="configs">The infrastructure configurations to aggregate.</param>
    /// <param name="projectVariableGroups">The project-level pipeline variable groups.</param>
    /// <param name="agentPoolName">The optional self-hosted agent pool name.</param>
    /// <param name="bicepBasePath">The optional repository sub-path for Bicep artifacts.</param>
    /// <param name="pipelineBasePath">The optional repository sub-path for pipeline artifacts.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The assembled mono-repo pipeline output, or the first generation error encountered.</returns>
    Task<ErrorOr<MonoRepoPipelineResult>> GenerateAsync(
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        IReadOnlyCollection<ProjectPipelineVariableGroup> projectVariableGroups,
        string? agentPoolName,
        string? bicepBasePath,
        string? pipelineBasePath,
        CancellationToken cancellationToken = default);
}
