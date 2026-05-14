using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.DeletePrivateDnsZone;

/// <summary>Deletes a Private DNS Zone resource.</summary>
public record DeletePrivateDnsZoneCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
