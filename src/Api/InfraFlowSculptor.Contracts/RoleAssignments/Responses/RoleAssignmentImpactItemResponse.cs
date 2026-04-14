namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>Describes a single impact item when removing a role assignment.</summary>
/// <param name="ImpactType">Impact category: <c>AcrPullRequired</c>, <c>KeyVaultSecretsRequired</c>, or <c>LastRoleToTarget</c>.</param>
/// <param name="AffectedResourceId">Identifier of the resource that will be affected.</param>
/// <param name="AffectedResourceName">Display name of the affected resource.</param>
/// <param name="AffectedResourceType">Type of the affected resource (e.g. "FunctionApp").</param>
/// <param name="TargetResourceId">Identifier of the target resource on which the role is granted.</param>
/// <param name="TargetResourceName">Display name of the target resource.</param>
/// <param name="TargetResourceType">Type of the target resource (e.g. "ContainerRegistry").</param>
/// <param name="Description">Human-readable description of the impact.</param>
/// <param name="Severity">Impact severity: <c>Critical</c> or <c>Warning</c>.</param>
/// <param name="AffectedSettingsCount">Number of settings affected, or <c>null</c> if not applicable.</param>
public record RoleAssignmentImpactItemResponse(
    Guid AffectedResourceId,
    string AffectedResourceName,
    string AffectedResourceType,
    Guid TargetResourceId,
    string TargetResourceName,
    string TargetResourceType,
    string ImpactType,
    string Description,
    string Severity,
    int? AffectedSettingsCount
);
