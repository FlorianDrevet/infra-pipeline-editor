using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.UpdateResourceGroup;

/// <summary>Handles updating an existing resource group's name and location.</summary>
public class UpdateResourceGroupCommandHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateResourceGroupCommand, ResourceGroupResult>
{
    public async Task<ErrorOr<ResourceGroupResult>> Handle(UpdateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        resourceGroup.Update(request.Name, request.Location);

        return mapper.Map<ResourceGroupResult>(resourceGroup);
    }
}
