using InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Contracts.PrivateDnsZones.Requests;
using InfraFlowSculptor.Contracts.PrivateDnsZones.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster configuration for Private DNS Zone mappings.</summary>
public sealed class PrivateDnsZoneMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePrivateDnsZoneRequest, CreatePrivateDnsZoneCommand>();

        config.NewConfig<(Guid Id, UpdatePrivateDnsZoneRequest Request), UpdatePrivateDnsZoneCommand>()
            .MapWith(src => new UpdatePrivateDnsZoneCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>()));

        config.NewConfig<PrivateDnsZone, PrivateDnsZoneResult>()
            .Map(dest => dest.VirtualNetworkLinks,
                src => src.VirtualNetworkLinks.Select(l => new VirtualNetworkLinkData(
                    l.Id.Value.ToString(),
                    l.VirtualNetworkId.Value.ToString(),
                    l.EnableAutoRegistration)).ToList());

        config.NewConfig<VirtualNetworkLinkData, VirtualNetworkLinkResponse>()
            .MapWith(src => new VirtualNetworkLinkResponse(
                src.Id, src.VirtualNetworkId, src.EnableAutoRegistration));
    }
}
