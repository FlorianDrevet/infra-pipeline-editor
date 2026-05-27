namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;

/// <summary>Result indicating whether the compute resource has AcrPull access via a User Assigned Identity.</summary>
/// <param name="HasAccess">Whether the resource has the required role assignment via UAI.</param>
/// <param name="MissingRoleDefinitionId">The role definition ID that is missing, if access is not granted.</param>
/// <param name="MissingRoleName">The name of the missing role.</param>
/// <param name="AssignedUserAssignedIdentityId">ID of the UAI that has a role assignment on this container registry, if any.</param>
/// <param name="AssignedUserAssignedIdentityName">Name of the UAI that has a role assignment on this container registry, if any.</param>
/// <param name="HasUserAssignedIdentity">Whether the resource has any UAI-based role assignment at all.</param>
/// <param name="AcrAuthMode">Authentication mode used by the compute resource to access the container registry.</param>
public sealed record CheckAcrPullAccessResult(
    bool HasAccess,
    string? MissingRoleDefinitionId,
    string? MissingRoleName,
    string? AssignedUserAssignedIdentityId,
    string? AssignedUserAssignedIdentityName,
    bool HasUserAssignedIdentity,
    string? AcrAuthMode);
