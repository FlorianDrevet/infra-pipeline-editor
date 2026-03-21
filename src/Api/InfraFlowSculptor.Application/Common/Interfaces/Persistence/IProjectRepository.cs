using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for the <see cref="Project"/> aggregate root.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Retrieves a project by identifier, including its members and their user information.
    /// Returns <c>null</c> if not found.
    /// </summary>
    Task<Project?> GetByIdWithMembersAsync(ProjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all projects where the given user is a member.
    /// </summary>
    Task<List<Project>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project by identifier, including its members and associated configurations.
    /// Returns <c>null</c> if not found.
    /// </summary>
    Task<Project?> GetByIdWithConfigurationsAsync(ProjectId id, CancellationToken cancellationToken = default);
}
