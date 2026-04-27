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
/// Registered as <c>Scoped</c> — caches results per HTTP request to avoid redundant DB queries
/// when multiple handlers verify access to the same project within one request scope.
/// </summary>
internal sealed class ProjectAccessService(
    IProjectRepository projectRepository,
    ICurrentUser currentUser)
    : IProjectAccessService
{
    private readonly Dictionary<ProjectId, ErrorOr<Project>> _readCache = new();
    private readonly Dictionary<ProjectId, ErrorOr<Project>> _writeCache = new();
    private readonly Dictionary<ProjectId, ErrorOr<Project>> _ownerCache = new();

    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyReadAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        if (_readCache.TryGetValue(projectId, out var cached))
            return cached;

        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null || !project.Members.Any(m => m.UserId == userId))
        {
            var error = Errors.Project.NotFoundError(projectId);
            _readCache[projectId] = error;
            return error;
        }

        _readCache[projectId] = project;
        return project;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyWriteAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        if (_writeCache.TryGetValue(projectId, out var cached))
            return cached;

        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null)
        {
            var error = Errors.Project.NotFoundError(projectId);
            _writeCache[projectId] = error;
            return error;
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            var error = Errors.Project.NotFoundError(projectId);
            _writeCache[projectId] = error;
            return error;
        }

        if (member.Role.Value == Role.RoleEnum.Reader)
        {
            var error = Errors.Project.ForbiddenError();
            _writeCache[projectId] = error;
            return error;
        }

        _writeCache[projectId] = project;
        return project;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Project>> VerifyOwnerAccessAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        if (_ownerCache.TryGetValue(projectId, out var cached))
            return cached;

        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);

        if (project is null)
        {
            var error = Errors.Project.NotFoundError(projectId);
            _ownerCache[projectId] = error;
            return error;
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            var error = Errors.Project.NotFoundError(projectId);
            _ownerCache[projectId] = error;
            return error;
        }

        if (member.Role.Value != Role.RoleEnum.Owner)
        {
            var error = Errors.Project.ForbiddenError();
            _ownerCache[projectId] = error;
            return error;
        }

        _ownerCache[projectId] = project;
        return project;
    }
}
