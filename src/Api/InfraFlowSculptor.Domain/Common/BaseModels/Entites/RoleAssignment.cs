using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>Represents an Azure RBAC role assignment between a source and target resource.</summary>
public sealed class RoleAssignment : Entity<RoleAssignmentId>
{
    /// <summary>Identifier of the Azure resource whose managed identity holds the role.</summary>
    public AzureResourceId SourceResourceId { get; private set; } = null!;

    /// <summary>Identifier of the Azure resource on which the role is granted.</summary>
    public AzureResourceId TargetResourceId { get; private set; } = null!;

    /// <summary>Type of managed identity used to perform the role assignment.</summary>
    public ManagedIdentityType ManagedIdentityType { get; private set; } = null!;

    /// <summary>Azure built-in role definition ID that was granted.</summary>
    public string RoleDefinitionId { get; private set; } = null!;

    /// <summary>
    /// Identifier of the User-Assigned Identity resource used for this role assignment.
    /// Only set when <see cref="ManagedIdentityType"/> is <c>UserAssigned</c>; <c>null</c> for system-assigned.
    /// </summary>
    public AzureResourceId? UserAssignedIdentityId { get; private set; }

    private RoleAssignment() { }

    internal RoleAssignment(
        AzureResourceId sourceResourceId,
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId,
        AzureResourceId? userAssignedIdentityId = null)
        : base(RoleAssignmentId.CreateUnique())
    {
        SourceResourceId = sourceResourceId;
        TargetResourceId = targetResourceId;
        ManagedIdentityType = managedIdentityType;
        RoleDefinitionId = roleDefinitionId;
        UserAssignedIdentityId = userAssignedIdentityId;
    }

    /// <summary>Creates a new <see cref="RoleAssignment"/> instance.</summary>
    /// <param name="sourceResourceId">Identifier of the source resource.</param>
    /// <param name="targetResourceId">Identifier of the target resource.</param>
    /// <param name="managedIdentityType">The managed identity type.</param>
    /// <param name="roleDefinitionId">The Azure role definition ID.</param>
    /// <param name="userAssignedIdentityId">Optional User-Assigned Identity resource ID (required when <paramref name="managedIdentityType"/> is UserAssigned).</param>
    /// <returns>A new <see cref="RoleAssignment"/> entity.</returns>
    internal static RoleAssignment Create(
        AzureResourceId sourceResourceId,
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId,
        AzureResourceId? userAssignedIdentityId = null)
        => new(sourceResourceId, targetResourceId, managedIdentityType, roleDefinitionId, userAssignedIdentityId);

    /// <summary>Updates the managed identity type and optional User-Assigned Identity for this role assignment.</summary>
    /// <param name="managedIdentityType">New managed identity type.</param>
    /// <param name="userAssignedIdentityId">New User-Assigned Identity resource ID, or <c>null</c> for system-assigned.</param>
    internal void UpdateIdentity(ManagedIdentityType managedIdentityType, AzureResourceId? userAssignedIdentityId)
    {
        ManagedIdentityType = managedIdentityType;
        UserAssignedIdentityId = userAssignedIdentityId;
    }
}
