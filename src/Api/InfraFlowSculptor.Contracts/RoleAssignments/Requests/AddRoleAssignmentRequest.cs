using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Contracts.RoleAssignments.Requests;

/// <summary>Request body for assigning an Azure RBAC role to a target resource using the managed identity of the source resource.</summary>
public class AddRoleAssignmentRequest
{
    /// <summary>Unique identifier of the Azure resource that will receive the role assignment (the target).</summary>
    [Required, GuidValidation]
    public required Guid TargetResourceId { get; init; }

    /// <summary>
    /// Type of managed identity used to perform the role assignment.
    /// Accepted values: <c>SystemAssigned</c>, <c>UserAssigned</c>.
    /// </summary>
    [Required, EnumValidation(typeof(ManagedIdentityType.IdentityTypeEnum))]
    public required string ManagedIdentityType { get; init; }

    /// <summary>
    /// Azure built-in role definition ID to grant (e.g. <c>/providers/Microsoft.Authorization/roleDefinitions/…</c>).
    /// Use <c>GET /available-role-definitions</c> to retrieve valid IDs for the source resource type.
    /// </summary>
    [Required]
    public required string RoleDefinitionId { get; init; }

    /// <summary>Required when <see cref="ManagedIdentityType"/> is <c>UserAssigned</c>. ID of the User-Assigned Identity resource to use.</summary>
    public Guid? UserAssignedIdentityId { get; init; }
}
