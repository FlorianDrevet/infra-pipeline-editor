using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;

public class ListResourceGroupsByConfigQueryHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<ListResourceGroupsByConfigQuery, ErrorOr<List<ResourceGroupResult>>>
{
    public async Task<ErrorOr<List<ResourceGroupResult>>> Handle(
        ListResourceGroupsByConfigQuery query, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, query.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.InfrastructureConfig.NotFoundError(query.InfraConfigId);

        var resourceGroups = await resourceGroupRepository.GetByInfraConfigIdAsync(
            query.InfraConfigId, cancellationToken);

        return resourceGroups.Select(rg => mapper.Map<ResourceGroupResult>(rg)).ToList();
    }
}
