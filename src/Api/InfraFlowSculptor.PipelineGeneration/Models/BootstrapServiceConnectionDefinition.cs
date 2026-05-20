namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Represents a service connection that must exist in Azure DevOps before pipelines can run.
/// Used by the bootstrap pipeline to validate existence.
/// </summary>
/// <param name="Name">The exact Azure DevOps service connection name.</param>
/// <param name="Type">The service connection type category (e.g. <c>AzureRM</c>, <c>DockerRegistry</c>).</param>
/// <param name="Environment">The environment this SC is associated with, or <c>null</c> for global connections.</param>
public sealed record BootstrapServiceConnectionDefinition(
    string Name,
    string Type,
    string? Environment);
