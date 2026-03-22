using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Enforces access control at the project level.
/// </summary>
internal sealed class ProjectAccessService(
    IProjectRepository projectRepository,
    ICurrentUser currentUser)
    : IProjectAccessService
{
    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyReadAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

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
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Errors.Project.NotFoundError(projectId);

        if (member.Role.Value == Role.RoleEnum.Reader)
            return Errors.Project.ForbiddenError();

        return project;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyOwnerAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Errors.Project.NotFoundError(projectId);

        if (member.Role.Value != Role.RoleEnum.Owner)
            return Errors.Project.ForbiddenError();

        return project;
    }
}
