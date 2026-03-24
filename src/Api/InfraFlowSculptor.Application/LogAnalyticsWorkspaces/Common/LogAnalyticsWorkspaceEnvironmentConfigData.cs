namespace InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;

/// <summary>
/// Carries typed per-environment Log Analytics Workspace data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record LogAnalyticsWorkspaceEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? RetentionInDays,
    decimal? DailyQuotaGb);
