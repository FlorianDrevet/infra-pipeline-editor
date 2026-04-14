using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Contracts.RoleAssignments.Requests;

/// <summary>Request body for updating the managed identity on an existing role assignment.</summary>
public class UpdateRoleAssignmentIdentityRequest
{
    /// <summary>
    /// Type of managed identity to use.
    /// Accepted values: <c>SystemAssigned</c>, <c>UserAssigned</c>.
    /// </summary>
    [Required, EnumValidation(typeof(ManagedIdentityType.IdentityTypeEnum))]
    public required string ManagedIdentityType { get; init; }

    /// <summary>Required when <see cref="ManagedIdentityType"/> is <c>UserAssigned</c>. ID of the User-Assigned Identity resource to use.</summary>
    public Guid? UserAssignedIdentityId { get; init; }
}
