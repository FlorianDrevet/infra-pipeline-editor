using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateSubnet;

/// <summary>Handles the <see cref="UpdateSubnetCommand"/> request.</summary>
public sealed class UpdateSubnetCommandHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateSubnetCommand, VirtualNetworkResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<VirtualNetworkResult>> Handle(
        UpdateSubnetCommand request,
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

        var name = Name.Create(request.Name);
        var delegation = request.Delegation is not null
            ? new SubnetDelegation(Enum.Parse<SubnetDelegation.Delegation>(request.Delegation, ignoreCase: true))
            : (SubnetDelegation?)null;
        var privateEndpointPolicy = new PrivateEndpointNetworkPolicy(
            Enum.Parse<PrivateEndpointNetworkPolicy.Policy>(request.PrivateEndpointNetworkPolicies, ignoreCase: true));
        var nsgId = request.NsgId is not null
            ? new AzureResourceId(Guid.Parse(request.NsgId))
            : (AzureResourceId?)null;

        vnet.UpdateSubnet(new SubnetId(request.SubnetId), name, request.AddressPrefix, delegation, request.ServiceEndpoints, privateEndpointPolicy, nsgId);

        virtualNetworkRepository.Update(vnet);

        return mapper.Map<VirtualNetworkResult>(vnet);
    }
}
