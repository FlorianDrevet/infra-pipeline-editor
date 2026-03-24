namespace InfraFlowSculptor.Application.AppConfigurations.Common;

/// <summary>
/// Carries typed per-environment App Configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Sku">Optional pricing tier override (e.g., "Free", "Standard").</param>
/// <param name="SoftDeleteRetentionInDays">Optional soft-delete retention period in days.</param>
/// <param name="PurgeProtectionEnabled">Optional purge protection flag.</param>
/// <param name="DisableLocalAuth">Optional flag to disable local authentication.</param>
/// <param name="PublicNetworkAccess">Optional public network access setting (e.g., "Enabled", "Disabled").</param>
public record AppConfigurationEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? SoftDeleteRetentionInDays,
    bool? PurgeProtectionEnabled,
    bool? DisableLocalAuth,
    string? PublicNetworkAccess);
