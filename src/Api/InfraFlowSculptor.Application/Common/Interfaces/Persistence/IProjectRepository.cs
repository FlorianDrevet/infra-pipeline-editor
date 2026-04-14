using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;

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
}
