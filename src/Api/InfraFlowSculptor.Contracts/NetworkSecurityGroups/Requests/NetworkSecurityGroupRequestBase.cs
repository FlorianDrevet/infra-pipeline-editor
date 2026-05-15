using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.NetworkSecurityGroups.Requests;

/// <summary>Common properties shared by create and update Network Security Group requests.</summary>
public abstract class NetworkSecurityGroupRequestBase
{
    /// <summary>Display name for the NSG resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the NSG will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }
}
