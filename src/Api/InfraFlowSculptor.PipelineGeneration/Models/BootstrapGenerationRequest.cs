namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Aggregated request for the bootstrap pipeline generation engine.
/// Contains all data needed to emit a self-contained <c>bootstrap.pipeline.yml</c> that
/// provisions Azure DevOps pipeline definitions, variable groups, and their authorizations.
/// </summary>
public sealed record BootstrapGenerationRequest
{
    /// <summary>Gets the Azure DevOps organization name (without trailing slash).</summary>
    public required string OrganizationName { get; init; }

    /// <summary>Gets the Azure DevOps project name.</summary>
    public required string ProjectName { get; init; }

    /// <summary>Gets the Azure DevOps Git repository name.</summary>
    public required string RepositoryName { get; init; }

    /// <summary>Gets the default branch used when creating pipelines (e.g. <c>main</c> or <c>develop</c>).</summary>
    public required string DefaultBranch { get; init; }

    /// <summary>
    /// Gets the self-hosted agent pool name. When <c>null</c>, the Microsoft-hosted pool
    /// (<c>vmImage: ubuntu-latest</c>) is used.
    /// </summary>
    public string? AgentPoolName { get; init; }

    /// <summary>Gets the list of Azure DevOps pipeline definitions to be created by the bootstrap pipeline.</summary>
    public IReadOnlyList<BootstrapPipelineDefinition> Pipelines { get; init; } = [];

    /// <summary>Gets the list of Azure DevOps variable groups to be created by the bootstrap pipeline.</summary>
    public IReadOnlyList<BootstrapVariableGroupDefinition> VariableGroups { get; init; } = [];
}
