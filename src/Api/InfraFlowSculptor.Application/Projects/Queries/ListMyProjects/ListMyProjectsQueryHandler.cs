using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;

/// <summary>Handles the <see cref="ListMyProjectsQuery"/>.</summary>
public sealed class ListMyProjectsQueryHandler(
    IProjectRepository repository,
    ICurrentUser currentUser)
    : IQueryHandler<ListMyProjectsQuery, List<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<ProjectResult>>> Handle(
        ListMyProjectsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var projects = await repository.GetAllForUserAsync(userId, cancellationToken);
        return projects.Select(ProjectResultMapper.ToProjectResult).ToList();
    }
}
