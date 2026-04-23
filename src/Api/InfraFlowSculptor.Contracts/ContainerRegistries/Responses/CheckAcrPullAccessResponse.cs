namespace InfraFlowSculptor.Contracts.ContainerRegistries.Responses;

/// <summary>Response DTO for the ACR pull access check.</summary>
public record CheckAcrPullAccessResponse(
    bool HasAccess,
    string? MissingRoleDefinitionId,
    string? MissingRoleName,
    string? AssignedUserAssignedIdentityId,
    string? AssignedUserAssignedIdentityName,
    bool HasUserAssignedIdentity,
    string? AcrAuthMode);
