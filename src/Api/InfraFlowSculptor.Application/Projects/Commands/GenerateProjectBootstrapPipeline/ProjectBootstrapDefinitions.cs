using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>
/// Groups the bootstrap pipeline, variable-group, and environment definitions assembled for a project.
/// </summary>
/// <param name="InfraPipelines">The infrastructure bootstrap pipeline definitions.</param>
/// <param name="AppPipelines">The application bootstrap pipeline definitions.</param>
/// <param name="VariableGroups">The Azure DevOps variable groups to provision.</param>
/// <param name="Environments">The Azure DevOps environments to provision.</param>
public sealed record ProjectBootstrapDefinitions(
    IReadOnlyList<BootstrapPipelineDefinition> InfraPipelines,
    IReadOnlyList<BootstrapPipelineDefinition> AppPipelines,
    IReadOnlyList<BootstrapVariableGroupDefinition> VariableGroups,
    IReadOnlyList<BootstrapEnvironmentDefinition> Environments);