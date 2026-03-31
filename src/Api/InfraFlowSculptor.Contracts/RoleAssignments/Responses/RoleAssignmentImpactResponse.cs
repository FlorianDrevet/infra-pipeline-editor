namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>Response DTO for the role assignment impact analysis endpoint.</summary>
/// <param name="HasImpact">Whether removal of the role assignment would cause any impact.</param>
/// <param name="Impacts">Detailed list of impacts, empty when <paramref name="HasImpact"/> is false.</param>
public record RoleAssignmentImpactResponse(
    bool HasImpact,
    List<RoleAssignmentImpactItemResponse> Impacts
);
