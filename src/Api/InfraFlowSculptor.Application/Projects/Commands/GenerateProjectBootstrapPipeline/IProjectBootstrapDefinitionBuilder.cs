using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.ProjectAggregate;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>
/// Builds the project-level bootstrap definitions consumed by bootstrap artifact generation.
/// </summary>
public interface IProjectBootstrapDefinitionBuilder
{
    /// <summary>
    /// Builds pipeline, variable-group, and environment definitions for the supplied project and configurations.
    /// </summary>
    /// <param name="project">The loaded project aggregate with pipeline variable groups and environments.</param>
    /// <param name="configs">The infrastructure configurations that belong to the project.</param>
    /// <param name="infraPipelineBasePath">The base path used for infrastructure bootstrap YAML files.</param>
    /// <param name="appPipelineBasePath">The base path used for application bootstrap YAML files.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The assembled bootstrap definitions for generation.</returns>
    Task<ProjectBootstrapDefinitions> BuildAsync(
        Project project,
        IReadOnlyList<InfrastructureConfigReadModel> configs,
        string? infraPipelineBasePath,
        string? appPipelineBasePath,
        CancellationToken cancellationToken);
}