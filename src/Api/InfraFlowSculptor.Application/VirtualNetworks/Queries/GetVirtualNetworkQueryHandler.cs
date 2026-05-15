using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.VirtualNetworks.Queries;

/// <summary>Handles retrieval of a single Virtual Network.</summary>
public sealed class GetVirtualNetworkQueryHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetVirtualNetworkQuery, VirtualNetworkResult>
{
    public async Task<ErrorOr<VirtualNetworkResult>> Handle(GetVirtualNetworkQuery query, CancellationToken cancellationToken)
    {
        var vnet = await virtualNetworkRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (vnet is null)
            return Errors.VirtualNetwork.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(vnet.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.VirtualNetwork.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.VirtualNetwork.NotFoundError(query.Id);

        return mapper.Map<VirtualNetworkResult>(vnet);
    }
}
