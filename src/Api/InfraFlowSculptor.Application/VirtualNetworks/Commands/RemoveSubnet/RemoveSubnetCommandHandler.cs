using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.RemoveSubnet;

/// <summary>Handles the <see cref="RemoveSubnetCommand"/> request.</summary>
public sealed class RemoveSubnetCommandHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<RemoveSubnetCommand, VirtualNetworkResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<VirtualNetworkResult>> Handle(
        RemoveSubnetCommand request,
        CancellationToken cancellationToken)
    {
        var vnet = await virtualNetworkRepository.GetByIdAsync(request.VirtualNetworkId, cancellationToken);
        if (vnet is null)
            return Errors.VirtualNetwork.NotFoundError(request.VirtualNetworkId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(vnet.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.VirtualNetwork.NotFoundError(request.VirtualNetworkId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        vnet.RemoveSubnet(new SubnetId(request.SubnetId));

        virtualNetworkRepository.Update(vnet);

        return mapper.Map<VirtualNetworkResult>(vnet);
    }
}
