using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.DeleteVirtualNetwork;

/// <summary>Handles deletion of a Virtual Network.</summary>
public sealed class DeleteVirtualNetworkCommandHandler(
    IVirtualNetworkRepository virtualNetworkRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteVirtualNetworkCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteVirtualNetworkCommand request, CancellationToken cancellationToken)
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

        await virtualNetworkRepository.DeleteAsync(request.Id);
        return Result.Deleted;
    }
}
