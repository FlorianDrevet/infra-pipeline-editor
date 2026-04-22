using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ResourceGroups.Requests;

/// <summary>Request body for creating a new Azure Resource Group inside an Infrastructure Configuration.</summary>
public class CreateResourceGroupRequest
{
    /// <summary>Unique identifier of the Infrastructure Configuration that will own this Resource Group.</summary>
    [Required, GuidValidation] 
    public required Guid InfraConfigId { get; init; }

    /// <summary>Display name for the Resource Group.</summary>
    [Required] public required string Name { get; init; }

    /// <summary>Azure region where the Resource Group will be created (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
