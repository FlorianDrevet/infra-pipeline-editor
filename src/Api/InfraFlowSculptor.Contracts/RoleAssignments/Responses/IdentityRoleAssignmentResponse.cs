namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>
/// Response DTO for a role assignment viewed from the identity perspective.
/// Enriched with source and target resource names and types.
/// </summary>
/// <param name="Id">Unique identifier of the role assignment.</param>
/// <param name="SourceResourceId">Identifier of the source Azure resource that holds the role.</param>
/// <param name="SourceResourceName">Display name of the source resource.</param>
/// <param name="SourceResourceType">Type of the source resource.</param>
/// <param name="TargetResourceId">Identifier of the target Azure resource on which the role is granted.</param>
/// <param name="TargetResourceName">Display name of the target resource.</param>
/// <param name="TargetResourceType">Type of the target resource.</param>
/// <param name="RoleDefinitionId">Azure role definition ID that was granted.</param>
/// <param name="RoleName">Human-readable name of the role.</param>
public record IdentityRoleAssignmentResponse(
    Guid Id,
    Guid SourceResourceId,
    string SourceResourceName,
    string SourceResourceType,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string RoleDefinitionId,
    string RoleName
);
