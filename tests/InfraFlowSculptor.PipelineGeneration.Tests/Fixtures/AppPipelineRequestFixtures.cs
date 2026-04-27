using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

/// <summary>
/// Synthetic, deterministic fixtures for <see cref="AppPipelineGenerationRequest"/>.
/// All values are hardcoded to guarantee byte-for-byte stable golden file capture
/// for the <c>AppPipelineGenerationEngine</c>.
/// </summary>
internal static class AppPipelineRequestFixtures
{
    private const string SubscriptionDev = "00000000-0000-0000-0000-000000000001";
    private const string ConfigName = "core";

    /// <summary>Container App resource using ServiceConnection ACR auth.</summary>
    public static AppPipelineGenerationRequest ContainerApp() => new()
    {
        ResourceName = "myapi",
        ApplicationName = "MyApi",
        ResourceType = AzureResourceTypes.ContainerApp,
        DeploymentMode = DeploymentModes.Container,
        DockerfilePath = "src/Api/Dockerfile",
        DockerImageName = "myapi",
        ContainerRegistryName = "ifsacr",
        AcrAuthMode = AcrAuthModes.ManagedIdentity,
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Web App resource deployed in code mode (.NET 8).</summary>
    public static AppPipelineGenerationRequest WebAppCode() => new()
    {
        ResourceName = "myweb",
        ApplicationName = "MyWeb",
        ResourceType = AzureResourceTypes.WebApp,
        DeploymentMode = DeploymentModes.Code,
        SourceCodePath = "src/Web",
        RuntimeStack = "DOTNETCORE",
        RuntimeVersion = "8.0",
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Web App resource in Container mode using ManagedIdentity ACR auth.</summary>
    public static AppPipelineGenerationRequest WebAppContainer_ManagedIdentity() => new()
    {
        ResourceName = "myweb",
        ApplicationName = "MyWeb",
        ResourceType = AzureResourceTypes.WebApp,
        DeploymentMode = DeploymentModes.Container,
        DockerfilePath = "src/Web/Dockerfile",
        DockerImageName = "myweb",
        ContainerRegistryName = "ifsacr",
        AcrAuthMode = AcrAuthModes.ManagedIdentity,
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Web App resource in Container mode using AdminCredentials ACR auth.</summary>
    public static AppPipelineGenerationRequest WebAppContainer_AdminCredentials() => new()
    {
        ResourceName = "myweb",
        ApplicationName = "MyWeb",
        ResourceType = AzureResourceTypes.WebApp,
        DeploymentMode = DeploymentModes.Container,
        DockerfilePath = "src/Web/Dockerfile",
        DockerImageName = "myweb",
        ContainerRegistryName = "ifsacr",
        AcrAuthMode = AcrAuthModes.AdminCredentials,
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Function App resource deployed in code mode (.NET 8).</summary>
    public static AppPipelineGenerationRequest FunctionAppCode() => new()
    {
        ResourceName = "myfunc",
        ApplicationName = "MyFunc",
        ResourceType = AzureResourceTypes.FunctionApp,
        DeploymentMode = DeploymentModes.Code,
        SourceCodePath = "src/Func",
        RuntimeStack = "DOTNETCORE",
        RuntimeVersion = "8.0",
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Function App resource in Container mode using ServiceConnection ACR auth.</summary>
    public static AppPipelineGenerationRequest FunctionAppContainer() => new()
    {
        ResourceName = "myfunc",
        ApplicationName = "MyFunc",
        ResourceType = AzureResourceTypes.FunctionApp,
        DeploymentMode = DeploymentModes.Container,
        DockerfilePath = "src/Func/Dockerfile",
        DockerImageName = "myfunc",
        ContainerRegistryName = "ifsacr",
        AcrAuthMode = AcrAuthModes.ManagedIdentity,
        ConfigName = ConfigName,
        IsMonoRepo = false,
        Environments = [BuildEnvironment("dev")],
    };

    /// <summary>Two compute resources sharing the same config name, used by combined-mode tests.</summary>
    public static IReadOnlyList<AppPipelineGenerationRequest> TwoComputeResourcesForCombined() =>
    [
        ContainerApp(),
        WebAppCode(),
    ];

    private static EnvironmentDefinition BuildEnvironment(string name) => new()
    {
        Name = name,
        ShortName = name,
        Location = "westeurope",
        Prefix = name,
        Suffix = name,
        AzureResourceManagerConnection = $"sc-{name}",
        SubscriptionId = SubscriptionDev,
    };
}
