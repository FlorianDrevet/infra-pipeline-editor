using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Queries;

/// <summary>Handles retrieval of a single Private DNS Zone.</summary>
public sealed class GetPrivateDnsZoneQueryHandler(
    IPrivateDnsZoneRepository privateDnsZoneRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetPrivateDnsZoneQuery, PrivateDnsZoneResult>
{
    public async Task<ErrorOr<PrivateDnsZoneResult>> Handle(GetPrivateDnsZoneQuery query, CancellationToken cancellationToken)
    {
        var zone = await privateDnsZoneRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (zone is null)
            return Errors.PrivateDnsZone.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(zone.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.PrivateDnsZone.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.PrivateDnsZone.NotFoundError(query.Id);

        return mapper.Map<PrivateDnsZoneResult>(zone);
    }
}
