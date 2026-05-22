using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;

/// <summary>
/// Handles listing all Azure resources across all configurations in a project.
/// Returns resource metadata enriched with the owning configuration context.
/// </summary>
public sealed class ListProjectResourcesQueryHandler(
    IProjectAccessService projectAccessService,
    IProjectResourceReadRepository projectResourceReadRepository)
    : IQueryHandler<ListProjectResourcesQuery, List<ProjectResourceResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<ProjectResourceResult>>> Handle(
        ListProjectResourcesQuery query,
        CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(query.ProjectId);
        var authResult = await projectAccessService.VerifyReadAccessAsync(projectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return await projectResourceReadRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }
}
