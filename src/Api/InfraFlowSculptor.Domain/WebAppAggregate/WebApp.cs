using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.WebAppAggregate;

/// <summary>Represents an Azure Web App resource.</summary>
public class WebApp : AzureResource
{
    private readonly List<WebAppEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Web App.</summary>
    public IReadOnlyCollection<WebAppEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the App Service Plan that hosts this Web App.</summary>
    public AzureResourceId AppServicePlanId { get; private set; } = null!;

    /// <summary>Gets the runtime stack configured for this Web App.</summary>
    public WebAppRuntimeStack RuntimeStack { get; private set; } = null!;

    /// <summary>Gets the runtime version (e.g., "8.0", "20").</summary>
    public string RuntimeVersion { get; private set; } = string.Empty;

    /// <summary>Gets whether the app is always on.</summary>
    public bool AlwaysOn { get; private set; }

    /// <summary>Gets whether the app requires HTTPS only.</summary>
    public bool HttpsOnly { get; private set; }

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages
        => Array.Empty<ParameterUsage>();

    private WebApp()
    {
    }

    /// <summary>Updates the mutable properties of this Web App.</summary>
    public void Update(
        Name name,
        Location location,
        AzureResourceId appServicePlanId,
        WebAppRuntimeStack runtimeStack,
        string runtimeVersion,
        bool alwaysOn,
        bool httpsOnly)
    {
        Name = name;
        Location = location;
        AppServicePlanId = appServicePlanId;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        AlwaysOn = alwaysOn;
        HttpsOnly = httpsOnly;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        bool? alwaysOn,
        bool? httpsOnly,
        WebAppRuntimeStack? runtimeStack,
        string? runtimeVersion)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(alwaysOn, httpsOnly, runtimeStack, runtimeVersion);
        }
        else
        {
            _environmentSettings.Add(
                WebAppEnvironmentSettings.Create(Id, environmentName, alwaysOn, httpsOnly, runtimeStack, runtimeVersion));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, bool? AlwaysOn, bool? HttpsOnly, WebAppRuntimeStack? RuntimeStack, string? RuntimeVersion)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                WebAppEnvironmentSettings.Create(Id, s.EnvironmentName, s.AlwaysOn, s.HttpsOnly, s.RuntimeStack, s.RuntimeVersion));
        }
    }

    /// <summary>Creates a new Web App with a generated identifier.</summary>
    public static WebApp Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId appServicePlanId,
        WebAppRuntimeStack runtimeStack,
        string runtimeVersion,
        bool alwaysOn,
        bool httpsOnly,
        IReadOnlyList<(string EnvironmentName, bool? AlwaysOn, bool? HttpsOnly, WebAppRuntimeStack? RuntimeStack, string? RuntimeVersion)>? environmentSettings = null)
    {
        var webApp = new WebApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            AppServicePlanId = appServicePlanId,
            RuntimeStack = runtimeStack,
            RuntimeVersion = runtimeVersion,
            AlwaysOn = alwaysOn,
            HttpsOnly = httpsOnly
        };

        if (environmentSettings is not null)
            webApp.SetAllEnvironmentSettings(environmentSettings);

        return webApp;
    }
}
