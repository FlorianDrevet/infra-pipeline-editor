namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>Application-layer result DTO wrapping role assignments with assigned identity info.</summary>
/// <param name="AssignedUserAssignedIdentityId">ID of the explicitly assigned UAI, or <c>null</c>.</param>
/// <param name="AssignedUserAssignedIdentityName">Display name of the assigned UAI, or <c>null</c>.</param>
/// <param name="RoleAssignments">The role assignments for the resource.</param>
public record RoleAssignmentsWithIdentityResult(
    string? AssignedUserAssignedIdentityId,
    string? AssignedUserAssignedIdentityName,
    List<RoleAssignmentResult> RoleAssignments);
