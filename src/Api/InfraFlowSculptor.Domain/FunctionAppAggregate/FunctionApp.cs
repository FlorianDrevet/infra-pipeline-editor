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
    private readonly List<FunctionAppEnvironmentSettings> _environmentSettings = [];

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

    /// <summary>Gets the deployment mode (Code or Container).</summary>
    public DeploymentMode DeploymentMode { get; private set; } = new(Common.ValueObjects.DeploymentMode.DeploymentModeType.Code);

    /// <summary>Gets the optional Container Registry identifier for container deployments.</summary>
    public AzureResourceId? ContainerRegistryId { get; private set; }

    /// <summary>Gets the Docker image name for container deployments (e.g., "myapp/func").</summary>
    public string? DockerImageName { get; private set; }

    /// <summary>Gets the optional relative path to the Dockerfile in the repository for container deployments.</summary>
    public string? DockerfilePath { get; private set; }

    /// <summary>Gets the optional relative path to the source code folder for code deployments.</summary>
    public string? SourceCodePath { get; private set; }

    /// <summary>Gets the optional custom build command for pipeline generation.</summary>
    public string? BuildCommand { get; private set; }

    /// <summary>Gets the user-friendly application name displayed in Azure DevOps pipeline runs.</summary>
    public string? ApplicationName { get; private set; }

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
        bool httpsOnly,
        DeploymentMode deploymentMode,
        AzureResourceId? containerRegistryId,
        string? dockerImageName,
        string? dockerfilePath,
        string? sourceCodePath,
        string? buildCommand,
        string? applicationName)
    {
        Name = name;
        Location = location;
        AppServicePlanId = appServicePlanId;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        HttpsOnly = httpsOnly;
        DeploymentMode = deploymentMode;
        ContainerRegistryId = containerRegistryId;
        DockerImageName = dockerImageName;
        DockerfilePath = dockerfilePath;
        SourceCodePath = sourceCodePath;
        BuildCommand = buildCommand;
        ApplicationName = applicationName;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        bool? httpsOnly,
        int? maxInstanceCount,
        string? dockerImageTag)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(httpsOnly, maxInstanceCount, dockerImageTag);
        }
        else
        {
            _environmentSettings.Add(
                FunctionAppEnvironmentSettings.Create(Id, environmentName, httpsOnly, maxInstanceCount, dockerImageTag));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, bool? HttpsOnly, int? MaxInstanceCount, string? DockerImageTag)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                FunctionAppEnvironmentSettings.Create(Id, s.EnvironmentName, s.HttpsOnly, s.MaxInstanceCount, s.DockerImageTag));
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
        DeploymentMode deploymentMode,
        AzureResourceId? containerRegistryId,
        string? dockerImageName,
        string? dockerfilePath = null,
        string? sourceCodePath = null,
        string? buildCommand = null,
        string? applicationName = null,
        IReadOnlyList<(string EnvironmentName, bool? HttpsOnly, int? MaxInstanceCount, string? DockerImageTag)>? environmentSettings = null)
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
            HttpsOnly = httpsOnly,
            DeploymentMode = deploymentMode,
            ContainerRegistryId = containerRegistryId,
            DockerImageName = dockerImageName,
            DockerfilePath = dockerfilePath,
            SourceCodePath = sourceCodePath,
            BuildCommand = buildCommand,
            ApplicationName = applicationName
        };

        if (environmentSettings is not null)
            functionApp.SetAllEnvironmentSettings(environmentSettings);

        return functionApp;
    }
}
