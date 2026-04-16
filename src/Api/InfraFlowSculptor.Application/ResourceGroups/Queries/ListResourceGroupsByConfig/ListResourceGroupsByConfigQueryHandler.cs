using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
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
            return authResult.Errors;

        var resourceGroups = await resourceGroupRepository.GetByInfraConfigIdAsync(
            query.InfraConfigId, cancellationToken);

        return resourceGroups.Select(rg => mapper.Map<ResourceGroupResult>(rg)).ToList();
    }
}
