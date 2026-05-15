using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;
using MapsterMapper;
using ErrorOr;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;

/// <summary>Handles creation of a Front Door.</summary>
public sealed class CreateFrontDoorCommandHandler(
    IFrontDoorRepository frontDoorRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateFrontDoorCommand, FrontDoorResult>
{
    public async Task<ErrorOr<FrontDoorResult>> Handle(CreateFrontDoorCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var fd = FrontDoor.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.WafPolicyEnabled,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, new FrontDoorSku(Enum.Parse<FrontDoorSku.Sku>(ec.Sku, ignoreCase: true))))
                .ToList(),
            isExisting: request.IsExisting);

        var saved = await frontDoorRepository.AddAsync(fd);
        return mapper.Map<FrontDoorResult>(saved);
    }
}
