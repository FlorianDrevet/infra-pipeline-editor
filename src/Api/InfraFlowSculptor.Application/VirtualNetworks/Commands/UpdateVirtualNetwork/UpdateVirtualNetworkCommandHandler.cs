using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;

/// <summary>Handles updating a Virtual Network.</summary>
public sealed class UpdateVirtualNetworkCommandHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateVirtualNetworkCommand, VirtualNetworkResult>
{
    public async Task<ErrorOr<VirtualNetworkResult>> Handle(UpdateVirtualNetworkCommand request, CancellationToken cancellationToken)
    {
        var vnet = await virtualNetworkRepository.GetByIdAsync(request.Id, cancellationToken);
        if (vnet is null)
            return Errors.VirtualNetwork.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(vnet.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.VirtualNetwork.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        vnet.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            vnet.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.AddressSpaces, ec.DnsServers, ec.EnableDdosProtection))
                    .ToList());

        var updated = virtualNetworkRepository.Update(vnet);
        return mapper.Map<VirtualNetworkResult>(updated);
    }
}
