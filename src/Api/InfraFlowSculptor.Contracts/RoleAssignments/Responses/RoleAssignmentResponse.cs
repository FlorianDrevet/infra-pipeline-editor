namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>Represents an RBAC role assignment where the source resource's managed identity is granted a role on a target resource.</summary>
/// <param name="Id">Unique identifier of the role assignment.</param>
/// <param name="SourceResourceId">Identifier of the Azure resource whose managed identity holds the role.</param>
/// <param name="TargetResourceId">Identifier of the Azure resource on which the role is granted.</param>
/// <param name="ManagedIdentityType">Type of managed identity used (e.g. "SystemAssigned", "UserAssigned").</param>
/// <param name="RoleDefinitionId">Azure role definition ID that was granted.</param>
/// <param name="UserAssignedIdentityId">Identifier of the User-Assigned Identity resource, or <c>null</c> for system-assigned.</param>
public record RoleAssignmentResponse(
    string Id,
    string SourceResourceId,
    string TargetResourceId,
    string ManagedIdentityType,
    string RoleDefinitionId,
    string? UserAssignedIdentityId
);
