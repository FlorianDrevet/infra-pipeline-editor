using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;

/// <summary>
/// Request body for updating an existing user-assigned managed identity.
/// </summary>
public class UpdateUserAssignedIdentityRequest
{
    /// <summary>Gets the new resource name.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Gets the new Azure location.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }
}
