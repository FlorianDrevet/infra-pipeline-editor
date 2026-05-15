using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.PrivateDnsZones.Requests;

/// <summary>Common properties shared by create and update Private DNS Zone requests.</summary>
public abstract class PrivateDnsZoneRequestBase
{
    /// <summary>Display name for the Private DNS Zone resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Private DNS Zone will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }
}
