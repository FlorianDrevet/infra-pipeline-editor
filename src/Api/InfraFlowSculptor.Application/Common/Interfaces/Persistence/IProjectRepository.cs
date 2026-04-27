using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="Project"/> aggregate root.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Retrieves a project by identifier, including its members and their user details.
    /// </summary>
    Task<Project?> GetByIdWithMembersAsync(ProjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project by identifier, including members, environments, and naming templates.
    /// </summary>
    Task<Project?> GetByIdWithAllAsync(ProjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project by identifier, including its pipeline variable groups and their mappings.
    /// </summary>
    Task<Project?> GetByIdWithPipelineVariableGroupsAsync(ProjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all projects the given user is a member of.
    /// </summary>
    Task<List<Project>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns only the project identifiers for projects the given user is a member of.
    /// Use this projection when only IDs are needed, avoiding loading full aggregates.
    /// </summary>
    Task<List<ProjectId>> GetProjectIdsForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns lightweight summaries (Id, Name, Description) for projects the given user is a member of.
    /// Use this projection when only summary fields are needed, avoiding loading full aggregates with navigation properties.
    /// </summary>
    Task<List<ProjectSummary>> GetProjectSummariesForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns pipeline variable usages (app settings, secure parameter mappings, app configuration keys)
    /// grouped by variable group identifier for the given set of variable groups.
    /// </summary>
    /// <param name="variableGroupIds">The variable group identifiers to query usages for.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A dictionary mapping each variable group identifier to its list of variable usages.</returns>
    Task<Dictionary<Guid, List<PipelineVariableUsageResult>>> GetPipelineVariableUsagesAsync(
        IReadOnlyCollection<ProjectPipelineVariableGroupId> variableGroupIds,
        CancellationToken cancellationToken = default);
}
