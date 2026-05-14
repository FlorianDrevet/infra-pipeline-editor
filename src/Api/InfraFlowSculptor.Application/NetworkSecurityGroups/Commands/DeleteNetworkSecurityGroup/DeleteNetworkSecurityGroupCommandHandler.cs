using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.DeleteNetworkSecurityGroup;

/// <summary>Handles deletion of a Network Security Group.</summary>
public sealed class DeleteNetworkSecurityGroupCommandHandler(
    INetworkSecurityGroupRepository nsgRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteNetworkSecurityGroupCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteNetworkSecurityGroupCommand request, CancellationToken cancellationToken)
    {
        var nsg = await nsgRepository.GetByIdAsync(request.Id, cancellationToken);
        if (nsg is null)
            return Errors.NetworkSecurityGroup.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(nsg.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.NetworkSecurityGroup.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await nsgRepository.DeleteAsync(request.Id);
        return Result.Deleted;
    }
}
