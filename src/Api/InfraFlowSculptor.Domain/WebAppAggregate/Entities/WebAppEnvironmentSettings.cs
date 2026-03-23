using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.WebAppAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="WebApp"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class WebAppEnvironmentSettings : Entity<WebAppEnvironmentSettingsId>
{
    /// <summary>Gets the parent Web App identifier.</summary>
    public AzureResourceId WebAppId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the always-on override for this environment.</summary>
    public bool? AlwaysOn { get; private set; }

    /// <summary>Gets or sets the HTTPS-only override for this environment.</summary>
    public bool? HttpsOnly { get; private set; }

    /// <summary>Gets or sets the runtime stack override for this environment.</summary>
    public WebAppRuntimeStack? RuntimeStack { get; private set; }

    /// <summary>Gets or sets the runtime version override for this environment (e.g., "8.0", "20").</summary>
    public string? RuntimeVersion { get; private set; }

    private WebAppEnvironmentSettings() { }

    internal WebAppEnvironmentSettings(
        AzureResourceId webAppId,
        string environmentName,
        bool? alwaysOn,
        bool? httpsOnly,
        WebAppRuntimeStack? runtimeStack,
        string? runtimeVersion)
        : base(WebAppEnvironmentSettingsId.CreateUnique())
    {
        WebAppId = webAppId;
        EnvironmentName = environmentName;
        AlwaysOn = alwaysOn;
        HttpsOnly = httpsOnly;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
    }

    /// <summary>
    /// Creates a new <see cref="WebAppEnvironmentSettings"/> for the specified web app and environment.
    /// </summary>
    public static WebAppEnvironmentSettings Create(
        AzureResourceId webAppId,
        string environmentName,
        bool? alwaysOn,
        bool? httpsOnly,
        WebAppRuntimeStack? runtimeStack,
        string? runtimeVersion)
        => new(webAppId, environmentName, alwaysOn, httpsOnly, runtimeStack, runtimeVersion);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(bool? alwaysOn, bool? httpsOnly, WebAppRuntimeStack? runtimeStack, string? runtimeVersion)
    {
        AlwaysOn = alwaysOn;
        HttpsOnly = httpsOnly;
        RuntimeStack = runtimeStack;
        RuntimeVersion = runtimeVersion;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (AlwaysOn is not null) dict["alwaysOn"] = AlwaysOn.Value.ToString().ToLower();
        if (HttpsOnly is not null) dict["httpsOnly"] = HttpsOnly.Value.ToString().ToLower();
        if (RuntimeStack is not null) dict["runtimeStack"] = RuntimeStack.Value.ToString().ToLower();
        if (RuntimeVersion is not null) dict["runtimeVersion"] = RuntimeVersion;
        return dict;
    }
}
