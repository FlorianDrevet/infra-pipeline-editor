namespace InfraFlowSculptor.Contracts.UserAssignedIdentities.Responses;

/// <summary>
/// API response for a user-assigned managed identity resource.
/// </summary>
public record UserAssignedIdentityResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location
);
