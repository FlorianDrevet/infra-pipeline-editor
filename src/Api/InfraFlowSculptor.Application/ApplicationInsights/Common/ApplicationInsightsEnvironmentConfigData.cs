namespace InfraFlowSculptor.Application.ApplicationInsights.Common;

/// <summary>
/// Carries typed per-environment Application Insights configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record ApplicationInsightsEnvironmentConfigData(
    string EnvironmentName,
    decimal? SamplingPercentage,
    int? RetentionInDays,
    bool? DisableIpMasking,
    bool? DisableLocalAuth,
    string? IngestionMode);
