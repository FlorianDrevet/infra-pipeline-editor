namespace InfraFlowSculptor.Contracts.RoleAssignments.Requests;

/// <summary>Request body to assign a User-Assigned Identity to a resource.</summary>
/// <param name="UserAssignedIdentityId">The ID of the UAI to assign.</param>
public record AssignIdentityToResourceRequest(Guid UserAssignedIdentityId);
