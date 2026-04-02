namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>
/// Wrapper response that includes the resource's assigned User-Assigned Identity
/// alongside its role assignments.
/// </summary>
/// <param name="AssignedUserAssignedIdentityId">ID of the explicitly assigned UAI, or <c>null</c> if none.</param>
/// <param name="AssignedUserAssignedIdentityName">Display name of the assigned UAI, or <c>null</c> if none.</param>
/// <param name="RoleAssignments">The role assignments for the resource.</param>
public record RoleAssignmentsWithIdentityResponse(
    string? AssignedUserAssignedIdentityId,
    string? AssignedUserAssignedIdentityName,
    List<RoleAssignmentResponse> RoleAssignments);
