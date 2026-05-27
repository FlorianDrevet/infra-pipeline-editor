using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using MapsterMapper;
using ErrorOr;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;

/// <summary>Handles creation of a Private DNS Zone.</summary>
public sealed class CreatePrivateDnsZoneCommandHandler(
    IPrivateDnsZoneRepository privateDnsZoneRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreatePrivateDnsZoneCommand, PrivateDnsZoneResult>
{
    public async Task<ErrorOr<PrivateDnsZoneResult>> Handle(CreatePrivateDnsZoneCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var zone = PrivateDnsZone.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            isExisting: request.IsExisting);

        var saved = privateDnsZoneRepository.Add(zone);
        return mapper.Map<PrivateDnsZoneResult>(saved);
    }
}
