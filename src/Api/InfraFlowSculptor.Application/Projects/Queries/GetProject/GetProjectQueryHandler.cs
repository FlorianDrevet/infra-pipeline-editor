using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProject;

/// <summary>Handles the <see cref="GetProjectQuery"/>.</summary>
public sealed class GetProjectQueryHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IResourceGroupRepository resourceGroupRepository)
    : IQueryHandler<GetProjectQuery, ProjectResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        GetProjectQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(query.Id, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        // Reload with full includes (members, environments, naming templates)
        var project = await projectRepository.GetByIdWithAllAsync(query.Id, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(query.Id);

        var usedResourceTypes = await resourceGroupRepository.GetDistinctResourceTypesByProjectIdAsync(
            query.Id, cancellationToken);

        var result = ProjectResultMapper.ToProjectResult(project);
        return result with { UsedResourceTypes = usedResourceTypes };
    }
}
