using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

/// <summary>Common properties shared by create and update Storage Account requests.</summary>
public abstract class StorageAccountRequestBase
{
    /// <summary>Display name for the Storage Account resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Storage Account will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<StorageAccountEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Storage Account.</summary>
public class StorageAccountEnvironmentConfigEntry
{
    /// <summary>Name of the target environment.</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional performance/replication tier override.</summary>
    [EnumValidation(typeof(StorageAccountSku.Sku))]
    public string? Sku { get; init; }

    /// <summary>Optional account kind override.</summary>
    [EnumValidation(typeof(StorageAccountKind.Kind))]
    public string? Kind { get; init; }

    /// <summary>Optional access tier override.</summary>
    [EnumValidation(typeof(StorageAccessTier.Tier))]
    public string? AccessTier { get; init; }

    /// <summary>Optional public blob access override.</summary>
    public bool? AllowBlobPublicAccess { get; init; }

    /// <summary>Optional HTTPS-only traffic override.</summary>
    public bool? EnableHttpsTrafficOnly { get; init; }

    /// <summary>Optional minimum TLS version override.</summary>
    [EnumValidation(typeof(StorageAccountTlsVersion.Version))]
    public string? MinimumTlsVersion { get; init; }
}

/// <summary>Response DTO for a typed per-environment Storage Account configuration.</summary>
public record StorageAccountEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    string? Kind,
    string? AccessTier,
    bool? AllowBlobPublicAccess,
    bool? EnableHttpsTrafficOnly,
    string? MinimumTlsVersion);
