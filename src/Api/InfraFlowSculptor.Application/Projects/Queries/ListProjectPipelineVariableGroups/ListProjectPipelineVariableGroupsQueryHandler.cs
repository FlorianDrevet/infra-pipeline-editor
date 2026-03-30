using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Handles listing project-level pipeline variable groups with their mappings.</summary>
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

        return project.PipelineVariableGroups
            .Select(g => new ProjectPipelineVariableGroupResult(
                g.Id.Value,
                g.GroupName,
                g.Mappings
                    .Select(m => new ProjectPipelineVariableMappingResult(
                        m.Id.Value,
                        m.PipelineVariableName,
                        m.BicepParameterName))
                    .ToList()))
            .ToList();
    }
}
