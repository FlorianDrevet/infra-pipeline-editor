using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="ContainerRegistry"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class ContainerRegistryEnvironmentSettings : Entity<ContainerRegistryEnvironmentSettingsId>
{
    /// <summary>Gets the parent Container Registry identifier.</summary>
    public AzureResourceId ContainerRegistryId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override (e.g., "Basic", "Standard", "Premium").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets whether the admin user is enabled.</summary>
    public bool? AdminUserEnabled { get; private set; }

    /// <summary>Gets or sets the public network access setting (e.g., "Enabled", "Disabled").</summary>
    public string? PublicNetworkAccess { get; private set; }

    /// <summary>Gets or sets whether zone redundancy is enabled (Premium only).</summary>
    public bool? ZoneRedundancy { get; private set; }

    private ContainerRegistryEnvironmentSettings() { }

    internal ContainerRegistryEnvironmentSettings(
        AzureResourceId containerRegistryId,
        string environmentName,
        string? sku,
        bool? adminUserEnabled,
        string? publicNetworkAccess,
        bool? zoneRedundancy)
        : base(ContainerRegistryEnvironmentSettingsId.CreateUnique())
    {
        ContainerRegistryId = containerRegistryId;
        EnvironmentName = environmentName;
        Sku = sku;
        AdminUserEnabled = adminUserEnabled;
        PublicNetworkAccess = publicNetworkAccess;
        ZoneRedundancy = zoneRedundancy;
    }

    /// <summary>
    /// Creates a new <see cref="ContainerRegistryEnvironmentSettings"/> for the specified Container Registry and environment.
    /// </summary>
    public static ContainerRegistryEnvironmentSettings Create(
        AzureResourceId containerRegistryId,
        string environmentName,
        string? sku,
        bool? adminUserEnabled,
        string? publicNetworkAccess,
        bool? zoneRedundancy)
        => new(containerRegistryId, environmentName, sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        bool? adminUserEnabled,
        string? publicNetworkAccess,
        bool? zoneRedundancy)
    {
        Sku = sku;
        AdminUserEnabled = adminUserEnabled;
        PublicNetworkAccess = publicNetworkAccess;
        ZoneRedundancy = zoneRedundancy;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (AdminUserEnabled is not null) dict["adminUserEnabled"] = AdminUserEnabled.Value.ToString().ToLower();
        if (PublicNetworkAccess is not null) dict["publicNetworkAccess"] = PublicNetworkAccess;
        if (ZoneRedundancy is not null) dict["zoneRedundancy"] = ZoneRedundancy.Value.ToString().ToLower();
        return dict;
    }
}
