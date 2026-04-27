using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Handles listing project-level pipeline variable groups with their variable usages.</summary>
public sealed class ListProjectPipelineVariableGroupsQueryHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : IQueryHandler<ListProjectPipelineVariableGroupsQuery, List<ProjectPipelineVariableGroupResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<ProjectPipelineVariableGroupResult>>> Handle(
        ListProjectPipelineVariableGroupsQuery query,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(query.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(query.ProjectId);

        var variableGroupIds = project.PipelineVariableGroups
            .Select(g => g.Id)
            .ToList();

        var usagesMap = await projectRepository.GetPipelineVariableUsagesAsync(variableGroupIds, cancellationToken);

        return project.PipelineVariableGroups
            .Select(g => new ProjectPipelineVariableGroupResult(
                g.Id.Value,
                g.GroupName,
                usagesMap.TryGetValue(g.Id.Value, out var usages) ? usages : []))
            .ToList();
    }
}
