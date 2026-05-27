using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects;

/// <summary>
/// Provides lightweight project resource projections without materializing polymorphic resource aggregates.
/// </summary>
public interface IProjectResourceReadRepository
{
    /// <summary>
    /// Gets all Azure resources belonging to the project across its infrastructure configurations.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Projected resource metadata enriched with resource group and configuration context.</returns>
    Task<List<ProjectResourceResult>> GetByProjectIdAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default);
}