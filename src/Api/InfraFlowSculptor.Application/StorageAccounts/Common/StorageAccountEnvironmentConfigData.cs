namespace InfraFlowSculptor.Application.StorageAccounts.Common;

/// <summary>
/// Carries typed per-environment Storage Account configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record StorageAccountEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    string? Kind,
    string? AccessTier,
    bool? AllowBlobPublicAccess,
    bool? EnableHttpsTrafficOnly,
    string? MinimumTlsVersion);
