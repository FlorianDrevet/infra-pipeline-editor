using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;

/// <summary>
/// Handles listing all Azure resources across all configurations in a project.
/// Returns resource metadata enriched with the owning configuration context.
/// </summary>
public sealed class ListProjectResourcesQueryHandler(
    IProjectAccessService projectAccessService,
    IInfrastructureConfigRepository infraConfigRepository,
    IResourceGroupRepository resourceGroupRepository)
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

        // Load all configs in the project
        var configs = await infraConfigRepository.GetByProjectIdAsync(projectId, cancellationToken);

        var results = new List<ProjectResourceResult>();

        foreach (var config in configs)
        {
            // GetByInfraConfigIdAsync already loads Resources via Include — use them directly
            // instead of re-loading each resource group individually (N+1 elimination).
            var resourceGroups = await resourceGroupRepository.GetByInfraConfigIdAsync(
                config.Id, cancellationToken);

            foreach (var rg in resourceGroups)
            {
                if (rg.Resources is null) continue;

                foreach (var resource in rg.Resources)
                {
                    results.Add(new ProjectResourceResult(
                        ResourceId: resource.Id.Value,
                        ResourceName: resource.Name.Value,
                        ResourceType: resource.GetType().Name,
                        ResourceGroupName: rg.Name.Value,
                        ConfigId: config.Id.Value,
                        ConfigName: config.Name.Value));
                }
            }
        }

        return results;
    }
}
