using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateDnsZones.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Queries;

/// <summary>Retrieves a single Private DNS Zone by identifier.</summary>
public record GetPrivateDnsZoneQuery(
    AzureResourceId Id
) : IQuery<PrivateDnsZoneResult>;
