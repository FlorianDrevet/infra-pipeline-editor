using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.DeletePrivateDnsZone;

/// <summary>Handles deletion of a Private DNS Zone.</summary>
public sealed class DeletePrivateDnsZoneCommandHandler(
    IPrivateDnsZoneRepository privateDnsZoneRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeletePrivateDnsZoneCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeletePrivateDnsZoneCommand request, CancellationToken cancellationToken)
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

        await privateDnsZoneRepository.DeleteAsync(request.Id);
        return Result.Deleted;
    }
}
