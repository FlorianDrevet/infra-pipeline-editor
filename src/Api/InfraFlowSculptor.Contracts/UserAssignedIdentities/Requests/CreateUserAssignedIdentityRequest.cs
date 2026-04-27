using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;

/// <summary>
/// Request body for creating a new user-assigned managed identity.
/// </summary>
public class CreateUserAssignedIdentityRequest
{
    /// <summary>Gets the resource name.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Gets the Azure location.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Gets the parent resource group identifier.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
