using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate;

/// <summary>Represents an Azure App Service Plan resource.</summary>
public sealed class AppServicePlan : AzureResource
{
    private readonly List<AppServicePlanEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this App Service Plan.</summary>
    public IReadOnlyCollection<AppServicePlanEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    /// <summary>Gets or sets the operating system type (Windows or Linux).</summary>
    public AppServicePlanOsType OsType { get; private set; } = null!;

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages
        => Array.Empty<ParameterUsage>();

    private AppServicePlan()
    {
    }

    /// <summary>Updates the mutable properties of this App Service Plan.</summary>
    public void Update(Name name, Location location, AppServicePlanOsType osType)
    {
        Name = name;
        Location = location;
        OsType = osType;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        AppServicePlanSku? sku,
        int? capacity)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, capacity);
        }
        else
        {
            _environmentSettings.Add(
                AppServicePlanEnvironmentSettings.Create(Id, environmentName, sku, capacity));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, AppServicePlanSku? Sku, int? Capacity)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, capacity) in settings)
        {
            _environmentSettings.Add(
                AppServicePlanEnvironmentSettings.Create(Id, envName, sku, capacity));
        }
    }

    /// <summary>Creates a new App Service Plan with a generated identifier.</summary>
    public static AppServicePlan Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AppServicePlanOsType osType,
        IReadOnlyList<(string EnvironmentName, AppServicePlanSku? Sku, int? Capacity)>? environmentSettings = null)
    {
        var plan = new AppServicePlan
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            OsType = osType
        };

        if (environmentSettings is not null)
            plan.SetAllEnvironmentSettings(environmentSettings);

        return plan;
    }
}
