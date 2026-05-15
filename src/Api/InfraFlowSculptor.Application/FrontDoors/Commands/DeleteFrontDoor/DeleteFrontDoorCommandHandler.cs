using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.DeleteFrontDoor;

/// <summary>Handles deletion of a Front Door.</summary>
public sealed class DeleteFrontDoorCommandHandler(
    IFrontDoorRepository frontDoorRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteFrontDoorCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteFrontDoorCommand request, CancellationToken cancellationToken)
    {
        var fd = await frontDoorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (fd is null)
            return Errors.FrontDoor.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(fd.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.FrontDoor.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await frontDoorRepository.DeleteAsync(request.Id);
        return Result.Deleted;
    }
}
