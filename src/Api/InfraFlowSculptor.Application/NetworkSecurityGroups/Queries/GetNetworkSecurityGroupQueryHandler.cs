using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Queries;

/// <summary>Handles retrieval of a single Network Security Group.</summary>
public sealed class GetNetworkSecurityGroupQueryHandler(
    INetworkSecurityGroupRepository nsgRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetNetworkSecurityGroupQuery, NetworkSecurityGroupResult>
{
    public async Task<ErrorOr<NetworkSecurityGroupResult>> Handle(GetNetworkSecurityGroupQuery query, CancellationToken cancellationToken)
    {
        var nsg = await nsgRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (nsg is null)
            return Errors.NetworkSecurityGroup.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(nsg.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.NetworkSecurityGroup.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.NetworkSecurityGroup.NotFoundError(query.Id);

        return mapper.Map<NetworkSecurityGroupResult>(nsg);
    }
}
