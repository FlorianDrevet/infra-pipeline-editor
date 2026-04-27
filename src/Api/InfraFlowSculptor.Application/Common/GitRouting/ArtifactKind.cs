namespace InfraFlowSculptor.Application.Common.GitRouting;

/// <summary>
/// Categorizes the kind of artifact being pushed to / generated for a Git repository.
/// Used by <see cref="IRepositoryTargetResolver"/> to pick the appropriate sub-path
/// (infra vs pipeline) from the effective <see cref="InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.RepositoryBinding"/>
/// and <see cref="InfraFlowSculptor.Domain.ProjectAggregate.Entities.ProjectRepository"/>.
/// </summary>
public enum ArtifactKind
{
    /// <summary>Infrastructure-as-Code artifacts (Bicep files, parameter files).</summary>
    Infrastructure,

    /// <summary>CI/CD pipeline artifacts (YAML pipelines per configuration).</summary>
    Pipeline,

    /// <summary>
    /// Project-level bootstrap pipeline artifacts targeting the infrastructure-flagged repository.
    /// In <c>SplitInfraCode</c> layout, this bootstrap owns the project-level shared Azure DevOps
    /// resources (environments, variable groups) and the infrastructure pipeline definitions.
    /// </summary>
    Bootstrap,

    /// <summary>Application CI/CD pipeline artifacts (per-resource wrappers + shared app templates) that target the application-code repository.</summary>
    ApplicationPipeline,

    /// <summary>
    /// Project-level bootstrap pipeline artifacts targeting the application-code-flagged repository.
    /// Only emitted in <c>SplitInfraCode</c> layout; provisions only application pipeline definitions
    /// and validates that shared project-level resources (environments, variable groups) already exist.
    /// </summary>
    BootstrapApplication,
}
