using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;

/// <summary>Handles updating a Network Security Group.</summary>
public sealed class UpdateNetworkSecurityGroupCommandHandler(
    INetworkSecurityGroupRepository nsgRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateNetworkSecurityGroupCommand, NetworkSecurityGroupResult>
{
    public async Task<ErrorOr<NetworkSecurityGroupResult>> Handle(UpdateNetworkSecurityGroupCommand request, CancellationToken cancellationToken)
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

        nsg.Update(request.Name, request.Location);

        var updated = nsgRepository.Update(nsg);
        return mapper.Map<NetworkSecurityGroupResult>(updated);
    }
}
