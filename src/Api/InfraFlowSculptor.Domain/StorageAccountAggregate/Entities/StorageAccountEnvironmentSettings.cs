using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="StorageAccount"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class StorageAccountEnvironmentSettings : Entity<StorageAccountEnvironmentSettingsId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the performance/replication tier override.</summary>
    public StorageAccountSku? Sku { get; private set; }

    /// <summary>Gets or sets the account kind override.</summary>
    public StorageAccountKind? Kind { get; private set; }

    /// <summary>Gets or sets the default blob access tier override.</summary>
    public StorageAccessTier? AccessTier { get; private set; }

    /// <summary>Gets or sets whether public blob access is allowed for this environment.</summary>
    public bool? AllowBlobPublicAccess { get; private set; }

    /// <summary>Gets or sets whether HTTPS-only traffic is enforced for this environment.</summary>
    public bool? EnableHttpsTrafficOnly { get; private set; }

    /// <summary>Gets or sets the minimum TLS version override.</summary>
    public StorageAccountTlsVersion? MinimumTlsVersion { get; private set; }

    private StorageAccountEnvironmentSettings() { }

    internal StorageAccountEnvironmentSettings(
        AzureResourceId storageAccountId,
        string environmentName,
        StorageAccountSku? sku,
        StorageAccountKind? kind,
        StorageAccessTier? accessTier,
        bool? allowBlobPublicAccess,
        bool? enableHttpsTrafficOnly,
        StorageAccountTlsVersion? minimumTlsVersion)
        : base(StorageAccountEnvironmentSettingsId.CreateUnique())
    {
        StorageAccountId = storageAccountId;
        EnvironmentName = environmentName;
        Sku = sku;
        Kind = kind;
        AccessTier = accessTier;
        AllowBlobPublicAccess = allowBlobPublicAccess;
        EnableHttpsTrafficOnly = enableHttpsTrafficOnly;
        MinimumTlsVersion = minimumTlsVersion;
    }

    /// <summary>
    /// Creates a new <see cref="StorageAccountEnvironmentSettings"/> for the specified Storage Account and environment.
    /// </summary>
    public static StorageAccountEnvironmentSettings Create(
        AzureResourceId storageAccountId,
        string environmentName,
        StorageAccountSku? sku,
        StorageAccountKind? kind,
        StorageAccessTier? accessTier,
        bool? allowBlobPublicAccess,
        bool? enableHttpsTrafficOnly,
        StorageAccountTlsVersion? minimumTlsVersion)
        => new(storageAccountId, environmentName, sku, kind, accessTier, allowBlobPublicAccess, enableHttpsTrafficOnly, minimumTlsVersion);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        StorageAccountSku? sku,
        StorageAccountKind? kind,
        StorageAccessTier? accessTier,
        bool? allowBlobPublicAccess,
        bool? enableHttpsTrafficOnly,
        StorageAccountTlsVersion? minimumTlsVersion)
    {
        Sku = sku;
        Kind = kind;
        AccessTier = accessTier;
        AllowBlobPublicAccess = allowBlobPublicAccess;
        EnableHttpsTrafficOnly = enableHttpsTrafficOnly;
        MinimumTlsVersion = minimumTlsVersion;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku.Value.ToString();
        if (Kind is not null) dict["kind"] = Kind.Value.ToString();
        if (AccessTier is not null) dict["accessTier"] = AccessTier.Value.ToString();
        if (AllowBlobPublicAccess is not null) dict["allowBlobPublicAccess"] = AllowBlobPublicAccess.Value.ToString().ToLower();
        if (EnableHttpsTrafficOnly is not null) dict["supportsHttpsTrafficOnly"] = EnableHttpsTrafficOnly.Value.ToString().ToLower();
        if (MinimumTlsVersion is not null)
        {
            dict["minimumTlsVersion"] = MinimumTlsVersion.Value switch
            {
                StorageAccountTlsVersion.Version.Tls10 => "TLS1_0",
                StorageAccountTlsVersion.Version.Tls11 => "TLS1_1",
                StorageAccountTlsVersion.Version.Tls12 => "TLS1_2",
                _ => "TLS1_2"
            };
        }
        return dict;
    }
}
