using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;

/// <summary>Updates an existing Private DNS Zone resource.</summary>
public record UpdatePrivateDnsZoneCommand(
    AzureResourceId Id,
    Name Name,
    Location Location
) : ICommand<PrivateDnsZoneResult>;
