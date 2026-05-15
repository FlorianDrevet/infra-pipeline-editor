using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using MapsterMapper;
using ErrorOr;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;

/// <summary>Handles creation of a Virtual Network.</summary>
public sealed class CreateVirtualNetworkCommandHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateVirtualNetworkCommand, VirtualNetworkResult>
{
    public async Task<ErrorOr<VirtualNetworkResult>> Handle(CreateVirtualNetworkCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var vnet = VirtualNetwork.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnableDdosProtection,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.AddressSpaces, ec.DnsServers))
                .ToList(),
            isExisting: request.IsExisting);

        var saved = await virtualNetworkRepository.AddAsync(vnet);
        return mapper.Map<VirtualNetworkResult>(saved);
    }
}
