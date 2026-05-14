using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;

/// <summary>Handles updating a Front Door.</summary>
public sealed class UpdateFrontDoorCommandHandler(
    IFrontDoorRepository frontDoorRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateFrontDoorCommand, FrontDoorResult>
{
    public async Task<ErrorOr<FrontDoorResult>> Handle(UpdateFrontDoorCommand request, CancellationToken cancellationToken)
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

        fd.Update(request.Name, request.Location, request.WafPolicyEnabled);

        if (request.EnvironmentSettings is not null)
            fd.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, new FrontDoorSku(Enum.Parse<FrontDoorSku.SkuEnum>(ec.Sku, ignoreCase: true))))
                    .ToList());

        var updated = await frontDoorRepository.UpdateAsync(fd);
        return mapper.Map<FrontDoorResult>(updated);
    }
}
