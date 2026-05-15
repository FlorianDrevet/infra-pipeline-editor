using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using MapsterMapper;
using ErrorOr;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;

/// <summary>Handles creation of a Network Security Group.</summary>
public sealed class CreateNetworkSecurityGroupCommandHandler(
    INetworkSecurityGroupRepository nsgRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateNetworkSecurityGroupCommand, NetworkSecurityGroupResult>
{
    public async Task<ErrorOr<NetworkSecurityGroupResult>> Handle(CreateNetworkSecurityGroupCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var nsg = NetworkSecurityGroup.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            isExisting: request.IsExisting);

        var saved = await nsgRepository.AddAsync(nsg);
        return mapper.Map<NetworkSecurityGroupResult>(saved);
    }
}
