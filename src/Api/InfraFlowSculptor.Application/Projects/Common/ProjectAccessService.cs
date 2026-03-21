using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Enforces project-level access control by checking membership and roles.
/// </summary>
internal sealed class ProjectAccessService(
    IProjectRepository repository,
    ICurrentUser currentUser)
    : IProjectAccessService
{
    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyReadAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await repository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null || !project.Members.Any(m => m.UserId == userId))
            return Errors.Project.NotFoundError(projectId);

        return project;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyWriteAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await repository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Errors.Project.NotFoundError(projectId);

        if (member.Role.Value == ProjectRole.ProjectRoleEnum.Reader)
            return Errors.Project.ForbiddenError();

        return project;
    }
}
