namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;

/// <summary>Aggregated availability result across all environments.</summary>
/// <param name="ResourceType">Resource type that was checked.</param>
/// <param name="RawName">Raw user-entered name that was checked.</param>
/// <param name="Supported">Whether availability checks are supported for this resource type.</param>
/// <param name="Environments">Per-environment results.</param>
public sealed record CheckResourceNameAvailabilityResult(
    string ResourceType,
    string RawName,
    bool Supported,
    IReadOnlyList<EnvironmentNameAvailabilityResult> Environments);

/// <summary>Per-environment availability result.</summary>
/// <param name="EnvironmentName">Display name of the environment.</param>
/// <param name="EnvironmentShortName">Short name of the environment.</param>
/// <param name="SubscriptionId">Azure subscription identifier of the environment.</param>
/// <param name="GeneratedName">Final generated resource name after template substitution.</param>
/// <param name="AppliedTemplate">Template that was applied.</param>
/// <param name="Status"><c>"available"</c> | <c>"unavailable"</c> | <c>"unknown"</c> | <c>"invalid"</c>.</param>
/// <param name="Reason">Optional Azure-provided reason code.</param>
/// <param name="Message">Optional human-readable diagnostic message.</param>
public sealed record EnvironmentNameAvailabilityResult(
    string EnvironmentName,
    string EnvironmentShortName,
    string SubscriptionId,
    string GeneratedName,
    string AppliedTemplate,
    string Status,
    string? Reason,
    string? Message);
