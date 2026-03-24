using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="ContainerAppEnvironment"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class ContainerAppEnvironmentEnvironmentSettings : Entity<ContainerAppEnvironmentEnvironmentSettingsId>
{
    /// <summary>Gets the parent Container App Environment identifier.</summary>
    public AzureResourceId ContainerAppEnvironmentId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override (e.g., "Consumption", "Premium").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets the workload profile type (e.g., "Consumption", "D4", "D8").</summary>
    public string? WorkloadProfileType { get; private set; }

    /// <summary>Gets or sets whether an internal load balancer is enabled.</summary>
    public bool? InternalLoadBalancerEnabled { get; private set; }

    /// <summary>Gets or sets whether zone redundancy is enabled.</summary>
    public bool? ZoneRedundancyEnabled { get; private set; }

    /// <summary>Gets or sets the Log Analytics workspace ID for diagnostics.</summary>
    public string? LogAnalyticsWorkspaceId { get; private set; }

    private ContainerAppEnvironmentEnvironmentSettings() { }

    internal ContainerAppEnvironmentEnvironmentSettings(
        AzureResourceId containerAppEnvironmentId,
        string environmentName,
        string? sku,
        string? workloadProfileType,
        bool? internalLoadBalancerEnabled,
        bool? zoneRedundancyEnabled,
        string? logAnalyticsWorkspaceId)
        : base(ContainerAppEnvironmentEnvironmentSettingsId.CreateUnique())
    {
        ContainerAppEnvironmentId = containerAppEnvironmentId;
        EnvironmentName = environmentName;
        Sku = sku;
        WorkloadProfileType = workloadProfileType;
        InternalLoadBalancerEnabled = internalLoadBalancerEnabled;
        ZoneRedundancyEnabled = zoneRedundancyEnabled;
        LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId;
    }

    /// <summary>
    /// Creates a new <see cref="ContainerAppEnvironmentEnvironmentSettings"/> for the specified Container App Environment and environment.
    /// </summary>
    public static ContainerAppEnvironmentEnvironmentSettings Create(
        AzureResourceId containerAppEnvironmentId,
        string environmentName,
        string? sku,
        string? workloadProfileType,
        bool? internalLoadBalancerEnabled,
        bool? zoneRedundancyEnabled,
        string? logAnalyticsWorkspaceId)
        => new(containerAppEnvironmentId, environmentName, sku, workloadProfileType, internalLoadBalancerEnabled, zoneRedundancyEnabled, logAnalyticsWorkspaceId);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        string? workloadProfileType,
        bool? internalLoadBalancerEnabled,
        bool? zoneRedundancyEnabled,
        string? logAnalyticsWorkspaceId)
    {
        Sku = sku;
        WorkloadProfileType = workloadProfileType;
        InternalLoadBalancerEnabled = internalLoadBalancerEnabled;
        ZoneRedundancyEnabled = zoneRedundancyEnabled;
        LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (WorkloadProfileType is not null) dict["workloadProfileType"] = WorkloadProfileType;
        if (InternalLoadBalancerEnabled is not null) dict["internalLoadBalancerEnabled"] = InternalLoadBalancerEnabled.Value.ToString().ToLower();
        if (ZoneRedundancyEnabled is not null) dict["zoneRedundancyEnabled"] = ZoneRedundancyEnabled.Value.ToString().ToLower();
        if (LogAnalyticsWorkspaceId is not null) dict["logAnalyticsWorkspaceId"] = LogAnalyticsWorkspaceId;
        return dict;
    }
}
