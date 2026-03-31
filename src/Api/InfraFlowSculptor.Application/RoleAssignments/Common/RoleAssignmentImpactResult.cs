namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>Result of analyzing the impact of removing a role assignment.</summary>
/// <param name="HasImpact">Whether removal of the role assignment would cause any impact.</param>
/// <param name="Impacts">Detailed list of impacts, empty when <paramref name="HasImpact"/> is false.</param>
public sealed record RoleAssignmentImpactResult(
    bool HasImpact,
    List<RoleAssignmentImpactItem> Impacts);
