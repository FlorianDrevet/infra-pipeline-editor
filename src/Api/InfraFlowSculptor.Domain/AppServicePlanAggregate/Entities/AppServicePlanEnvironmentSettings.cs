using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for an <see cref="AppServicePlan"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class AppServicePlanEnvironmentSettings : Entity<AppServicePlanEnvironmentSettingsId>
{
    /// <summary>Gets the parent App Service Plan identifier.</summary>
    public AzureResourceId AppServicePlanId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override for this environment.</summary>
    public AppServicePlanSku? Sku { get; private set; }

    /// <summary>Gets or sets the number of instances for this environment.</summary>
    public int? Capacity { get; private set; }

    private AppServicePlanEnvironmentSettings() { }

    internal AppServicePlanEnvironmentSettings(
        AzureResourceId appServicePlanId,
        string environmentName,
        AppServicePlanSku? sku,
        int? capacity)
        : base(AppServicePlanEnvironmentSettingsId.CreateUnique())
    {
        AppServicePlanId = appServicePlanId;
        EnvironmentName = environmentName;
        Sku = sku;
        Capacity = capacity;
    }

    /// <summary>
    /// Creates a new <see cref="AppServicePlanEnvironmentSettings"/> for the specified plan and environment.
    /// </summary>
    public static AppServicePlanEnvironmentSettings Create(
        AzureResourceId appServicePlanId,
        string environmentName,
        AppServicePlanSku? sku,
        int? capacity)
        => new(appServicePlanId, environmentName, sku, capacity);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(AppServicePlanSku? sku, int? capacity)
    {
        Sku = sku;
        Capacity = capacity;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku.Value.ToString();
        if (Capacity is not null) dict["capacity"] = Capacity.Value.ToString();
        return dict;
    }
}
