using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class CorsRuleEntry
{
    [Required]
    public required List<string> AllowedOrigins { get; init; }

    [Required]
    public required List<string> AllowedMethods { get; init; }

    [Required]
    public required List<string> AllowedHeaders { get; init; }

    [Required]
    public required List<string> ExposedHeaders { get; init; }

    [Range(0, int.MaxValue)]
    public int MaxAgeInSeconds { get; init; }
}

/// <summary>Common properties shared by create and update Storage Account requests.</summary>
public abstract class StorageAccountRequestBase
{
    /// <summary>Display name for the Storage Account resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Storage Account will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Storage account kind (e.g. StorageV2, BlobStorage).</summary>
    [Required, EnumValidation(typeof(StorageAccountKind.Kind))]
    public required string Kind { get; init; }

    /// <summary>Default access tier (Hot, Cool, Premium).</summary>
    [Required, EnumValidation(typeof(StorageAccessTier.Tier))]
    public required string AccessTier { get; init; }

    /// <summary>Whether public access to blobs is allowed.</summary>
    public bool AllowBlobPublicAccess { get; init; }

    /// <summary>Whether HTTPS-only traffic is enforced.</summary>
    public bool EnableHttpsTrafficOnly { get; init; } = true;

    /// <summary>Minimum TLS version for client connections.</summary>
    [Required, EnumValidation(typeof(StorageAccountTlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    /// <summary>Per-environment typed configuration overrides (SKU only).</summary>
    public List<StorageAccountEnvironmentConfigEntry>? EnvironmentSettings { get; init; }

    /// <summary>Blob service CORS rules applied after storage account deployment.</summary>
    public List<CorsRuleEntry>? CorsRules { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Storage Account. Only SKU varies per environment.</summary>
public class StorageAccountEnvironmentConfigEntry
{
    /// <summary>Name of the target environment.</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional performance/replication tier override.</summary>
    [EnumValidation(typeof(StorageAccountSku.Sku))]
    public string? Sku { get; init; }
}

/// <summary>Response DTO for a typed per-environment Storage Account configuration.</summary>
public record StorageAccountEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku);
