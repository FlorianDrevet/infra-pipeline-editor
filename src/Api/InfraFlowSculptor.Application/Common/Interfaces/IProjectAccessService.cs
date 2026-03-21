using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Service that enforces access control for Project resources.
/// </summary>
public interface IProjectAccessService
{
    /// <summary>
    /// Verifies the current user is a member of the given project (any role).
    /// Returns <c>NotFoundError</c> if the project does not exist or the user is not a member.
    /// </summary>
    Task<ErrorOr<Project>> VerifyReadAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the current user is an Owner or Contributor in the given project.
    /// Returns <c>NotFoundError</c> if the project does not exist or the user is not a member.
    /// Returns <c>ForbiddenError</c> if the user is a Reader.
    /// </summary>
    Task<ErrorOr<Project>> VerifyWriteAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default);
}
