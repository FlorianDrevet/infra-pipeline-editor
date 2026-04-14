using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;

/// <summary>Handles the <see cref="ListProjectConfigsQuery"/>.</summary>
public sealed class ListProjectConfigsQueryHandler(
    IProjectAccessService accessService,
    IInfrastructureConfigRepository configRepository,
    IResourceGroupRepository resourceGroupRepository,
    IMapper mapper)
    : IQueryHandler<ListProjectConfigsQuery, List<GetInfrastructureConfigResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListProjectConfigsQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var configs = await configRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);
        var results = configs.Select(c => mapper.Map<GetInfrastructureConfigResult>(c)).ToList();

        var configIds = configs.Select(c => c.Id).ToList();
        var counts = await resourceGroupRepository.GetResourceCountsByInfraConfigIdsAsync(configIds, cancellationToken);

        return results.Select(r =>
        {
            if (counts.TryGetValue(r.Id.Value, out var c))
                return r with { ResourceGroupCount = c.ResourceGroupCount, ResourceCount = c.ResourceCount };
            return r;
        }).ToList();
    }
}
