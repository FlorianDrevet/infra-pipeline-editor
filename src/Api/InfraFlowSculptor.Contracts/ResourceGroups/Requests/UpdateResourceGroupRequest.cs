using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ResourceGroups.Requests;

/// <summary>Request body for updating an existing Azure Resource Group.</summary>
public class UpdateResourceGroupRequest
{
    /// <summary>Display name for the Resource Group.</summary>
    [Required, StringLength(90)]
    public required string Name { get; init; }

    /// <summary>Azure region where the Resource Group will be located (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }
}
