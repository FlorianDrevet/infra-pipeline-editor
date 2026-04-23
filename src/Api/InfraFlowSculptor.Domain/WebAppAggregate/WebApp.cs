using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.WebAppAggregate;

/// <summary>Represents an Azure Web App resource.</summary>
public sealed class WebApp : AzureResource
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

    /// <summary>Gets the deployment mode (Code or Container).</summary>
    public DeploymentMode DeploymentMode { get; private set; } = new(Common.ValueObjects.DeploymentMode.DeploymentModeType.Code);

    /// <summary>Gets the optional Container Registry identifier for container deployments.</summary>
    public AzureResourceId? ContainerRegistryId { get; private set; }

    /// <summary>Gets the optional authentication mode used to pull images from Azure Container Registry.</summary>
    public AcrAuthMode? AcrAuthMode { get; private set; }

    /// <summary>Gets the Docker image name for container deployments (e.g., "myapp/api").</summary>
    public string? DockerImageName { get; private set; }

    /// <summary>Gets the optional relative path to the Dockerfile in the repository for container deployments.</summary>
    public string? DockerfilePath { get; private set; }

    /// <summary>Gets the optional relative path to the source code folder for code deployments.</summary>
    public string? SourceCodePath { get; private set; }

    /// <summary>Gets the optional custom build command for pipeline generation.</summary>
    public string? BuildCommand { get; private set; }

    /// <summary>Gets the user-friendly application name displayed in Azure DevOps pipeline runs.</summary>
    public string? ApplicationName { get; private set; }

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
        bool httpsOnly,
        DeploymentMode deploymentMode,
        AzureResourceId? containerRegistryId,
        AcrAuthMode? acrAuthMode,
        string? dockerImageName,
        string? dockerfilePath,
        string? sourceCodePath,
        string? buildCommand,
        string? applicationName)
    {
        Name = name;
        Location = location;

        if (IsExisting)
            return;

        AppServicePlanId = appServicePlanId;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        AlwaysOn = alwaysOn;
        HttpsOnly = httpsOnly;
        DeploymentMode = deploymentMode;
        ContainerRegistryId = containerRegistryId;
        AcrAuthMode = containerRegistryId is null ? null : acrAuthMode;
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
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetEnvironmentSettings(
        string environmentName,
        bool? alwaysOn,
        bool? httpsOnly,
        string? dockerImageTag)
    {
        if (IsExisting)
            return;
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(alwaysOn, httpsOnly, dockerImageTag);
        }
        else
        {
            _environmentSettings.Add(
                WebAppEnvironmentSettings.Create(Id, environmentName, alwaysOn, httpsOnly, dockerImageTag));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, bool? AlwaysOn, bool? HttpsOnly, string? DockerImageTag)> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                WebAppEnvironmentSettings.Create(Id, s.EnvironmentName, s.AlwaysOn, s.HttpsOnly, s.DockerImageTag));
        }
    }

    /// <summary>Creates a new Web App with a generated identifier.</summary>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    public static WebApp Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId appServicePlanId,
        WebAppRuntimeStack runtimeStack,
        string runtimeVersion,
        bool alwaysOn,
        bool httpsOnly,
        DeploymentMode deploymentMode,
        AzureResourceId? containerRegistryId,
        AcrAuthMode? acrAuthMode,
        string? dockerImageName,
        string? dockerfilePath = null,
        string? sourceCodePath = null,
        string? buildCommand = null,
        string? applicationName = null,
        IReadOnlyList<(string EnvironmentName, bool? AlwaysOn, bool? HttpsOnly, string? DockerImageTag)>? environmentSettings = null,
        bool isExisting = false)
    {
        var webApp = new WebApp
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            AppServicePlanId = appServicePlanId,
            RuntimeStack = runtimeStack,
            RuntimeVersion = runtimeVersion,
            AlwaysOn = alwaysOn,
            HttpsOnly = httpsOnly,
            DeploymentMode = deploymentMode,
            ContainerRegistryId = containerRegistryId,
            AcrAuthMode = containerRegistryId is null ? null : acrAuthMode,
            DockerImageName = dockerImageName,
            DockerfilePath = dockerfilePath,
            SourceCodePath = sourceCodePath,
            BuildCommand = buildCommand,
            ApplicationName = applicationName
        };

        if (!isExisting && environmentSettings is not null)
            webApp.SetAllEnvironmentSettings(environmentSettings);

        return webApp;
    }
}
