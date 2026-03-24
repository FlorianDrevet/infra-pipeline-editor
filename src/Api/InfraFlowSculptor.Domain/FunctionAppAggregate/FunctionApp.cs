using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.FunctionAppAggregate;

/// <summary>Represents an Azure Function App resource.</summary>
public class FunctionApp : AzureResource
{
    private readonly List<FunctionAppEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Function App.</summary>
    public IReadOnlyCollection<FunctionAppEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the App Service Plan that hosts this Function App.</summary>
    public AzureResourceId AppServicePlanId { get; private set; } = null!;

    /// <summary>Gets the runtime stack configured for this Function App.</summary>
    public FunctionAppRuntimeStack RuntimeStack { get; private set; } = null!;

    /// <summary>Gets the runtime version (e.g., "8.0", "20").</summary>
    public string RuntimeVersion { get; private set; } = string.Empty;

    /// <summary>Gets whether the app requires HTTPS only.</summary>
    public bool HttpsOnly { get; private set; }

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages
        => Array.Empty<ParameterUsage>();

    private FunctionApp()
    {
    }

    /// <summary>Updates the mutable properties of this Function App.</summary>
    public void Update(
        Name name,
        Location location,
        AzureResourceId appServicePlanId,
        FunctionAppRuntimeStack runtimeStack,
        string runtimeVersion,
        bool httpsOnly)
    {
        Name = name;
        Location = location;
        AppServicePlanId = appServicePlanId;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        HttpsOnly = httpsOnly;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        bool? httpsOnly,
        FunctionAppRuntimeStack? runtimeStack,
        string? runtimeVersion,
        int? maxInstanceCount,
        string? functionsWorkerRuntime)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(httpsOnly, runtimeStack, runtimeVersion, maxInstanceCount, functionsWorkerRuntime);
        }
        else
        {
            _environmentSettings.Add(
                FunctionAppEnvironmentSettings.Create(Id, environmentName, httpsOnly, runtimeStack, runtimeVersion, maxInstanceCount, functionsWorkerRuntime));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, bool? HttpsOnly, FunctionAppRuntimeStack? RuntimeStack, string? RuntimeVersion, int? MaxInstanceCount, string? FunctionsWorkerRuntime)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                FunctionAppEnvironmentSettings.Create(Id, s.EnvironmentName, s.HttpsOnly, s.RuntimeStack, s.RuntimeVersion, s.MaxInstanceCount, s.FunctionsWorkerRuntime));
        }
    }

    /// <summary>Creates a new Function App with a generated identifier.</summary>
    public static FunctionApp Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId appServicePlanId,
        FunctionAppRuntimeStack runtimeStack,
        string runtimeVersion,
        bool httpsOnly,
        IReadOnlyList<(string EnvironmentName, bool? HttpsOnly, FunctionAppRuntimeStack? RuntimeStack, string? RuntimeVersion, int? MaxInstanceCount, string? FunctionsWorkerRuntime)>? environmentSettings = null)
    {
        var functionApp = new FunctionApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            AppServicePlanId = appServicePlanId,
            RuntimeStack = runtimeStack,
            RuntimeVersion = runtimeVersion,
            HttpsOnly = httpsOnly
        };

        if (environmentSettings is not null)
            functionApp.SetAllEnvironmentSettings(environmentSettings);

        return functionApp;
    }
}
