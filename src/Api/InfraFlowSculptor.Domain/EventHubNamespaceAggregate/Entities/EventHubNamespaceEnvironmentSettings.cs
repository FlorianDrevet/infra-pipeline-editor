using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for an <see cref="EventHubNamespace"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class EventHubNamespaceEnvironmentSettings : Entity<EventHubNamespaceEnvironmentSettingsId>
{
    /// <summary>Gets the parent Event Hub Namespace identifier.</summary>
    public AzureResourceId EventHubNamespaceId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier (e.g., "Basic", "Standard", "Premium").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets the throughput or processing units capacity.</summary>
    public int? Capacity { get; private set; }

    /// <summary>Gets or sets whether zone redundancy is enabled.</summary>
    public bool? ZoneRedundant { get; private set; }

    /// <summary>Gets or sets whether local (SAS key) authentication is disabled.</summary>
    public bool? DisableLocalAuth { get; private set; }

    /// <summary>Gets or sets the minimum TLS version (e.g., "1.0", "1.1", "1.2").</summary>
    public string? MinimumTlsVersion { get; private set; }

    /// <summary>Gets or sets whether auto-inflate (automatic scaling) is enabled.</summary>
    public bool? AutoInflateEnabled { get; private set; }

    /// <summary>Gets or sets the maximum throughput units when auto-inflate is enabled (0-40).</summary>
    public int? MaxThroughputUnits { get; private set; }

    private EventHubNamespaceEnvironmentSettings() { }

    internal EventHubNamespaceEnvironmentSettings(
        AzureResourceId eventHubNamespaceId,
        string environmentName,
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion,
        bool? autoInflateEnabled,
        int? maxThroughputUnits)
        : base(EventHubNamespaceEnvironmentSettingsId.CreateUnique())
    {
        EventHubNamespaceId = eventHubNamespaceId;
        EnvironmentName = environmentName;
        Sku = sku;
        Capacity = capacity;
        ZoneRedundant = zoneRedundant;
        DisableLocalAuth = disableLocalAuth;
        MinimumTlsVersion = minimumTlsVersion;
        AutoInflateEnabled = autoInflateEnabled;
        MaxThroughputUnits = maxThroughputUnits;
    }

    /// <summary>
    /// Creates a new <see cref="EventHubNamespaceEnvironmentSettings"/> for the specified namespace and environment.
    /// </summary>
    public static EventHubNamespaceEnvironmentSettings Create(
        AzureResourceId eventHubNamespaceId,
        string environmentName,
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion,
        bool? autoInflateEnabled,
        int? maxThroughputUnits)
        => new(eventHubNamespaceId, environmentName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion, autoInflateEnabled, maxThroughputUnits);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion,
        bool? autoInflateEnabled,
        int? maxThroughputUnits)
    {
        Sku = sku;
        Capacity = capacity;
        ZoneRedundant = zoneRedundant;
        DisableLocalAuth = disableLocalAuth;
        MinimumTlsVersion = minimumTlsVersion;
        AutoInflateEnabled = autoInflateEnabled;
        MaxThroughputUnits = maxThroughputUnits;
    }

    /// <summary>Returns a dictionary of non-null environment configuration values.</summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (Capacity is not null) dict["capacity"] = Capacity.Value.ToString();
        if (ZoneRedundant is not null) dict["zoneRedundant"] = ZoneRedundant.Value.ToString().ToLower();
        if (DisableLocalAuth is not null) dict["disableLocalAuth"] = DisableLocalAuth.Value.ToString().ToLower();
        if (MinimumTlsVersion is not null) dict["minimumTlsVersion"] = MinimumTlsVersion;
        if (AutoInflateEnabled is not null) dict["autoInflateEnabled"] = AutoInflateEnabled.Value.ToString().ToLower();
        if (MaxThroughputUnits is not null) dict["maxThroughputUnits"] = MaxThroughputUnits.Value.ToString();
        return dict;
    }
}
