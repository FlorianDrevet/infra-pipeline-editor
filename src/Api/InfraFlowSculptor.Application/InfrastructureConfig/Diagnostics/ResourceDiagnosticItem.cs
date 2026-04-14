namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>A single diagnostic finding for a resource.</summary>
/// <param name="ResourceId">ID of the affected resource.</param>
/// <param name="ResourceName">Display name of the affected resource.</param>
/// <param name="ResourceType">ARM resource type string.</param>
/// <param name="Severity">Severity level.</param>
/// <param name="RuleCode">Stable code identifying the diagnostic rule (e.g. <c>"ACR_PULL_MISSING"</c>).</param>
/// <param name="TargetResourceName">Name of the related resource (e.g. the ACR or Key Vault).</param>
public record ResourceDiagnosticItem(
    Guid ResourceId,
    string ResourceName,
    string ResourceType,
    DiagnosticSeverity Severity,
    string RuleCode,
    string TargetResourceName);
