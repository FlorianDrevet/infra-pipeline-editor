using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="FunctionApp"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class FunctionAppEnvironmentSettings : Entity<FunctionAppEnvironmentSettingsId>
{
    /// <summary>Gets the parent Function App identifier.</summary>
    public AzureResourceId FunctionAppId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the HTTPS-only override for this environment.</summary>
    public bool? HttpsOnly { get; private set; }

    /// <summary>Gets or sets the runtime stack override for this environment.</summary>
    public FunctionAppRuntimeStack? RuntimeStack { get; private set; }

    /// <summary>Gets or sets the runtime version override for this environment (e.g., "8.0", "20").</summary>
    public string? RuntimeVersion { get; private set; }

    /// <summary>Gets or sets the maximum number of scale-out instances for this environment.</summary>
    public int? MaxInstanceCount { get; private set; }

    /// <summary>Gets or sets the Functions worker runtime override (e.g., "dotnet-isolated", "node", "python").</summary>
    public string? FunctionsWorkerRuntime { get; private set; }

    /// <summary>Gets or sets the Docker image tag override for this environment (e.g., "latest", "v1.2.3").</summary>
    public string? DockerImageTag { get; private set; }

    private FunctionAppEnvironmentSettings() { }

    internal FunctionAppEnvironmentSettings(
        AzureResourceId functionAppId,
        string environmentName,
        bool? httpsOnly,
        FunctionAppRuntimeStack? runtimeStack,
        string? runtimeVersion,
        int? maxInstanceCount,
        string? functionsWorkerRuntime,
        string? dockerImageTag)
        : base(FunctionAppEnvironmentSettingsId.CreateUnique())
    {
        FunctionAppId = functionAppId;
        EnvironmentName = environmentName;
        HttpsOnly = httpsOnly;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        MaxInstanceCount = maxInstanceCount;
        FunctionsWorkerRuntime = functionsWorkerRuntime;
        DockerImageTag = dockerImageTag;
    }

    /// <summary>
    /// Creates a new <see cref="FunctionAppEnvironmentSettings"/> for the specified function app and environment.
    /// </summary>
    public static FunctionAppEnvironmentSettings Create(
        AzureResourceId functionAppId,
        string environmentName,
        bool? httpsOnly,
        FunctionAppRuntimeStack? runtimeStack,
        string? runtimeVersion,
        int? maxInstanceCount,
        string? functionsWorkerRuntime,
        string? dockerImageTag)
        => new(functionAppId, environmentName, httpsOnly, runtimeStack, runtimeVersion, maxInstanceCount, functionsWorkerRuntime, dockerImageTag);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        bool? httpsOnly,
        FunctionAppRuntimeStack? runtimeStack,
        string? runtimeVersion,
        int? maxInstanceCount,
        string? functionsWorkerRuntime,
        string? dockerImageTag)
    {
        HttpsOnly = httpsOnly;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
        MaxInstanceCount = maxInstanceCount;
        FunctionsWorkerRuntime = functionsWorkerRuntime;
        DockerImageTag = dockerImageTag;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (HttpsOnly is not null) dict["httpsOnly"] = HttpsOnly.Value.ToString().ToLower();
        if (RuntimeStack is not null) dict["runtimeStack"] = RuntimeStack.Value.ToString().ToLower();
        if (RuntimeVersion is not null) dict["runtimeVersion"] = RuntimeVersion;
        if (MaxInstanceCount is not null) dict["maxInstanceCount"] = MaxInstanceCount.Value.ToString();
        if (FunctionsWorkerRuntime is not null) dict["functionsWorkerRuntime"] = FunctionsWorkerRuntime;
        if (DockerImageTag is not null) dict["dockerImageTag"] = DockerImageTag;
        return dict;
    }
}
