using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;

public class GetResourceGroupQueryHandler(IResourceGroupRepository resourceGroupRepository, IMapper mapper)
    : IRequestHandler<GetResourceGroupQuery, ErrorOr<ResourceGroupResult>>
{
    public async Task<ErrorOr<ResourceGroupResult>> Handle(GetResourceGroupQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(query.Id, cancellationToken);
        if (resourceGroup is null)
        {
            return Errors.ResourceGroup.NotFound(query.Id);
        }

        var result = mapper.Map<ResourceGroupResult>(resourceGroup);
        return result;
    }
}