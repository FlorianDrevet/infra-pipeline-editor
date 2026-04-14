namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Diagnostics report for an infrastructure configuration.</summary>
/// <param name="Diagnostics">The list of diagnostic findings.</param>
public record ConfigDiagnosticsResponse(IReadOnlyList<ResourceDiagnosticResponse> Diagnostics);

/// <summary>A single diagnostic finding for a specific resource.</summary>
/// <param name="ResourceId">ID of the affected resource.</param>
/// <param name="ResourceName">Display name of the affected resource.</param>
/// <param name="ResourceType">ARM resource type string.</param>
/// <param name="Severity">Severity level as string.</param>
/// <param name="RuleCode">Stable code identifying the diagnostic rule.</param>
/// <param name="TargetResourceName">Name of the related resource.</param>
public record ResourceDiagnosticResponse(
    string ResourceId,
    string ResourceName,
    string ResourceType,
    string Severity,
    string RuleCode,
    string TargetResourceName);
