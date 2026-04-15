using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;

/// <summary>
/// Returns all configurations across all projects the current user is a member of.
/// </summary>
public sealed class ListMyInfrastructureConfigsQueryHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    IResourceGroupRepository resourceGroupRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IQueryHandler<ListMyInfrastructureConfigsQuery, List<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListMyInfrastructureConfigsQuery query, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        // Use lightweight projection — only project IDs are needed, not full aggregates with Members/Users.
        var projectIds = await projectRepository.GetProjectIdsForUserAsync(userId, cancellationToken);

        var allConfigs = new List<GetInfrastructureConfigResult>();
        foreach (var projectId in projectIds)
        {
            var configs = await configRepository.GetByProjectIdAsync(projectId, cancellationToken);
            allConfigs.AddRange(configs.Select(c => mapper.Map<GetInfrastructureConfigResult>(c)));
        }

        var configIds = allConfigs.Select(c => c.Id).ToList();
        var counts = await resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(
            configIds,
            cancellationToken);

        return allConfigs.Select(r =>
        {
            if (counts.TryGetValue(r.Id.Value, out var c))
                return r with { ResourceGroupCount = c.ResourceGroupCount, ResourceCount = c.ResourceCount };
            return r;
        }).ToList();
    }
}
