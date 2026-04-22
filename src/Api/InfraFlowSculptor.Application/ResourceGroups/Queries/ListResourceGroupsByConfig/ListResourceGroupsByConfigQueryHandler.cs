using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;

public class ListResourceGroupsByConfigQueryHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<ListResourceGroupsByConfigQuery, List<ResourceGroupResult>>
{
    public async Task<ErrorOr<List<ResourceGroupResult>>> Handle(
        ListResourceGroupsByConfigQuery query, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(query.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.InfrastructureConfig.NotFoundError(query.InfraConfigId);

        var resourceGroups = await resourceGroupRepository.GetLightweightByInfraConfigIdAsync(
            query.InfraConfigId, cancellationToken);

        return resourceGroups.Select(rg => mapper.Map<ResourceGroupResult>(rg)).ToList();
    }
}
