using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;

/// <summary>Handles updating a Private DNS Zone.</summary>
public sealed class UpdatePrivateDnsZoneCommandHandler(
    IPrivateDnsZoneRepository privateDnsZoneRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdatePrivateDnsZoneCommand, PrivateDnsZoneResult>
{
    public async Task<ErrorOr<PrivateDnsZoneResult>> Handle(UpdatePrivateDnsZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await privateDnsZoneRepository.GetByIdAsync(request.Id, cancellationToken);
        if (zone is null)
            return Errors.PrivateDnsZone.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(zone.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.PrivateDnsZone.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        zone.Update(request.Name, request.Location);

        var updated = await privateDnsZoneRepository.UpdateAsync(zone);
        return mapper.Map<PrivateDnsZoneResult>(updated);
    }
}
