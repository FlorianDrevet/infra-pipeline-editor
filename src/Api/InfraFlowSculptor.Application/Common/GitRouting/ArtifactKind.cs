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

    /// <summary>Project-level bootstrap pipeline artifacts.</summary>
    Bootstrap,

    /// <summary>Application CI/CD pipeline artifacts (per-resource wrappers + shared app templates) that target the application-code repository.</summary>
    ApplicationPipeline,
}
