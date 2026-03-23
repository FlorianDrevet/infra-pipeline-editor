using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for an <see cref="AppConfiguration"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class AppConfigurationEnvironmentSettings : Entity<AppConfigurationEnvironmentSettingsId>
{
    /// <summary>Gets the parent App Configuration identifier.</summary>
    public AzureResourceId AppConfigurationId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override for this environment (e.g., "Free", "Standard").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets the soft-delete retention period in days.</summary>
    public int? SoftDeleteRetentionInDays { get; private set; }

    /// <summary>Gets or sets whether purge protection is enabled.</summary>
    public bool? PurgeProtectionEnabled { get; private set; }

    /// <summary>Gets or sets whether local authentication is disabled.</summary>
    public bool? DisableLocalAuth { get; private set; }

    /// <summary>Gets or sets the public network access setting (e.g., "Enabled", "Disabled").</summary>
    public string? PublicNetworkAccess { get; private set; }

    private AppConfigurationEnvironmentSettings() { }

    internal AppConfigurationEnvironmentSettings(
        AzureResourceId appConfigurationId,
        string environmentName,
        string? sku,
        int? softDeleteRetentionInDays,
        bool? purgeProtectionEnabled,
        bool? disableLocalAuth,
        string? publicNetworkAccess)
        : base(AppConfigurationEnvironmentSettingsId.CreateUnique())
    {
        AppConfigurationId = appConfigurationId;
        EnvironmentName = environmentName;
        Sku = sku;
        SoftDeleteRetentionInDays = softDeleteRetentionInDays;
        PurgeProtectionEnabled = purgeProtectionEnabled;
        DisableLocalAuth = disableLocalAuth;
        PublicNetworkAccess = publicNetworkAccess;
    }

    /// <summary>
    /// Creates a new <see cref="AppConfigurationEnvironmentSettings"/> for the specified App Configuration and environment.
    /// </summary>
    public static AppConfigurationEnvironmentSettings Create(
        AzureResourceId appConfigurationId,
        string environmentName,
        string? sku,
        int? softDeleteRetentionInDays,
        bool? purgeProtectionEnabled,
        bool? disableLocalAuth,
        string? publicNetworkAccess)
        => new(appConfigurationId, environmentName, sku, softDeleteRetentionInDays, purgeProtectionEnabled, disableLocalAuth, publicNetworkAccess);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        int? softDeleteRetentionInDays,
        bool? purgeProtectionEnabled,
        bool? disableLocalAuth,
        string? publicNetworkAccess)
    {
        Sku = sku;
        SoftDeleteRetentionInDays = softDeleteRetentionInDays;
        PurgeProtectionEnabled = purgeProtectionEnabled;
        DisableLocalAuth = disableLocalAuth;
        PublicNetworkAccess = publicNetworkAccess;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (SoftDeleteRetentionInDays is not null) dict["softDeleteRetentionInDays"] = SoftDeleteRetentionInDays.Value.ToString();
        if (PurgeProtectionEnabled is not null) dict["enablePurgeProtection"] = PurgeProtectionEnabled.Value.ToString().ToLower();
        if (DisableLocalAuth is not null) dict["disableLocalAuth"] = DisableLocalAuth.Value.ToString().ToLower();
        if (PublicNetworkAccess is not null) dict["publicNetworkAccess"] = PublicNetworkAccess;
        return dict;
    }
}
