using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.Common;
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

    /// <summary>Performance and replication tier. Accepted values: <c>Standard_LRS</c>, <c>Standard_GRS</c>, <c>Standard_RAGRS</c>, <c>Standard_ZRS</c>, <c>Premium_LRS</c>.</summary>
    [Required, EnumValidation(typeof(StorageAccountSku.Sku))]
    public required string Sku { get; init; }

    /// <summary>Account kind. Accepted values: <c>Storage</c>, <c>StorageV2</c>, <c>BlobStorage</c>, <c>BlockBlobStorage</c>, <c>FileStorage</c>.</summary>
    [Required, EnumValidation(typeof(StorageAccountKind.Kind))]
    public required string Kind { get; init; }

    /// <summary>Default access tier for blob data. Accepted values: <c>Hot</c>, <c>Cool</c>. Only applies to Blob/BlobStorage accounts.</summary>
    [Required, EnumValidation(typeof(StorageAccessTier.Tier))]
    public required string AccessTier { get; init; }

    /// <summary>When <c>false</c>, anonymous public read access to blobs and containers is disabled.</summary>
    [Required]
    public required bool AllowBlobPublicAccess { get; init; }

    /// <summary>When <c>true</c>, all HTTP traffic is redirected to HTTPS.</summary>
    [Required]
    public required bool EnableHttpsTrafficOnly { get; init; }

    /// <summary>Minimum accepted TLS version for requests to this storage account. Accepted values: <c>TLS1_0</c>, <c>TLS1_1</c>, <c>TLS1_2</c>.</summary>
    [Required, EnumValidation(typeof(StorageAccountTlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    /// <summary>Per-environment configuration overrides. Each entry specifies the properties for a specific deployment environment.</summary>
    public List<ResourceEnvironmentConfigEntry>? EnvironmentConfigs { get; init; }
}
