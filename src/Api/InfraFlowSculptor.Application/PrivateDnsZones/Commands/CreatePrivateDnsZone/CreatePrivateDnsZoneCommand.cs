using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;

/// <summary>Creates a new Private DNS Zone resource.</summary>
public record CreatePrivateDnsZoneCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool IsExisting = false
) : ICommand<PrivateDnsZoneResult>;
