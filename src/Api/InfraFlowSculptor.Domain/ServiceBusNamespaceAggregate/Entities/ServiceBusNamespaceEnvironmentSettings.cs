using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="ServiceBusNamespace"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class ServiceBusNamespaceEnvironmentSettings : Entity<ServiceBusNamespaceEnvironmentSettingsId>
{
    /// <summary>Gets the parent Service Bus Namespace identifier.</summary>
    public AzureResourceId ServiceBusNamespaceId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier (e.g., "Basic", "Standard", "Premium").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets the messaging units capacity (only applicable for Premium tier, 1-16).</summary>
    public int? Capacity { get; private set; }

    /// <summary>Gets or sets whether zone redundancy is enabled (Premium tier only).</summary>
    public bool? ZoneRedundant { get; private set; }

    /// <summary>Gets or sets whether local (SAS key) authentication is disabled.</summary>
    public bool? DisableLocalAuth { get; private set; }

    /// <summary>Gets or sets the minimum TLS version (e.g., "1.0", "1.1", "1.2").</summary>
    public string? MinimumTlsVersion { get; private set; }

    private ServiceBusNamespaceEnvironmentSettings() { }

    internal ServiceBusNamespaceEnvironmentSettings(
        AzureResourceId serviceBusNamespaceId,
        string environmentName,
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion)
        : base(ServiceBusNamespaceEnvironmentSettingsId.CreateUnique())
    {
        ServiceBusNamespaceId = serviceBusNamespaceId;
        EnvironmentName = environmentName;
        Sku = sku;
        Capacity = capacity;
        ZoneRedundant = zoneRedundant;
        DisableLocalAuth = disableLocalAuth;
        MinimumTlsVersion = minimumTlsVersion;
    }

    /// <summary>
    /// Creates a new <see cref="ServiceBusNamespaceEnvironmentSettings"/> for the specified namespace and environment.
    /// </summary>
    public static ServiceBusNamespaceEnvironmentSettings Create(
        AzureResourceId serviceBusNamespaceId,
        string environmentName,
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion)
        => new(serviceBusNamespaceId, environmentName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion)
    {
        Sku = sku;
        Capacity = capacity;
        ZoneRedundant = zoneRedundant;
        DisableLocalAuth = disableLocalAuth;
        MinimumTlsVersion = minimumTlsVersion;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (Capacity is not null) dict["capacity"] = Capacity.Value.ToString();
        if (ZoneRedundant is not null) dict["zoneRedundant"] = ZoneRedundant.Value.ToString().ToLower();
        if (DisableLocalAuth is not null) dict["disableLocalAuth"] = DisableLocalAuth.Value.ToString().ToLower();
        if (MinimumTlsVersion is not null) dict["minimumTlsVersion"] = MinimumTlsVersion;
        return dict;
    }
}
