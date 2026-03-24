using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.AppConfigurations.Requests;

/// <summary>Common properties shared by create and update App Configuration requests.</summary>
public abstract class AppConfigurationRequestBase
{
    /// <summary>Display name for the App Configuration resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the App Configuration will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<AppConfigurationEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for an App Configuration.</summary>
public class AppConfigurationEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override (e.g., "Free", "Standard").</summary>
    public string? Sku { get; init; }

    /// <summary>Optional soft-delete retention period in days (1–7).</summary>
    public int? SoftDeleteRetentionInDays { get; init; }

    /// <summary>Optional flag to enable purge protection.</summary>
    public bool? PurgeProtectionEnabled { get; init; }

    /// <summary>Optional flag to disable local authentication.</summary>
    public bool? DisableLocalAuth { get; init; }

    /// <summary>Optional public network access setting (e.g., "Enabled", "Disabled").</summary>
    public string? PublicNetworkAccess { get; init; }
}

/// <summary>Response DTO for a typed per-environment App Configuration configuration.</summary>
public record AppConfigurationEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? SoftDeleteRetentionInDays,
    bool? PurgeProtectionEnabled,
    bool? DisableLocalAuth,
    string? PublicNetworkAccess);
