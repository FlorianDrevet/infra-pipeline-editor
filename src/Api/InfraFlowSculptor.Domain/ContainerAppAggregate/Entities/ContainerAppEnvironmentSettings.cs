using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
using InfraFlowSculptor.Domain.ContainerAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="ContainerApp"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class ContainerAppEnvironmentSettings : Entity<ContainerAppEnvironmentSettingsId>
{
    /// <summary>Gets the parent Container App identifier.</summary>
    public AzureResourceId ContainerAppId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the CPU cores allocation (e.g., "0.25", "0.5", "1.0", "2.0").</summary>
    public string? CpuCores { get; private set; }

    /// <summary>Gets or sets the memory allocation (e.g., "0.5Gi", "1.0Gi", "2.0Gi").</summary>
    public string? MemoryGi { get; private set; }

    /// <summary>Gets or sets the minimum number of replicas.</summary>
    public int? MinReplicas { get; private set; }

    /// <summary>Gets or sets the maximum number of replicas.</summary>
    public int? MaxReplicas { get; private set; }

    /// <summary>Gets or sets whether ingress is enabled.</summary>
    public bool? IngressEnabled { get; private set; }

    /// <summary>Gets or sets the ingress target port.</summary>
    public int? IngressTargetPort { get; private set; }

    /// <summary>Gets or sets whether ingress is externally accessible.</summary>
    public bool? IngressExternal { get; private set; }

    /// <summary>Gets or sets the transport method (e.g., "auto", "http", "http2", "tcp").</summary>
    public string? TransportMethod { get; private set; }

    /// <summary>Gets or sets the readiness probe HTTP path (e.g., "/healthz/ready").</summary>
    public string? ReadinessProbePath { get; private set; }

    /// <summary>Gets or sets the readiness probe port.</summary>
    public int? ReadinessProbePort { get; private set; }

    /// <summary>Gets or sets the liveness probe HTTP path (e.g., "/healthz/live").</summary>
    public string? LivenessProbePath { get; private set; }

    /// <summary>Gets or sets the liveness probe port.</summary>
    public int? LivenessProbePort { get; private set; }

    /// <summary>Gets or sets the startup probe HTTP path (e.g., "/healthz/startup").</summary>
    public string? StartupProbePath { get; private set; }

    /// <summary>Gets or sets the startup probe port.</summary>
    public int? StartupProbePort { get; private set; }

    /// <summary>Gets the Azure DevOps Docker/ACR service connection name used by the application CI pipeline.</summary>
    public string? ContainerRegistryServiceConnection { get; private set; }

    private ContainerAppEnvironmentSettings() { }

    internal ContainerAppEnvironmentSettings(
        AzureResourceId containerAppId,
        ContainerAppEnvironmentSettingsData settings)
        : base(ContainerAppEnvironmentSettingsId.CreateUnique())
    {
        ContainerAppId = containerAppId;
        EnvironmentName = settings.EnvironmentName;
        Update(settings);
    }

    /// <summary>
    /// Creates a new <see cref="ContainerAppEnvironmentSettings"/> from typed per-environment configuration data.
    /// </summary>
    /// <param name="containerAppId">The parent Container App identifier.</param>
    /// <param name="settings">The typed per-environment settings to apply.</param>
    /// <returns>The initialized per-environment settings entity.</returns>
    public static ContainerAppEnvironmentSettings Create(
        AzureResourceId containerAppId,
        ContainerAppEnvironmentSettingsData settings)
        => new(containerAppId, settings);

    /// <summary>
    /// Updates the configuration overrides for this environment from typed data.
    /// </summary>
    /// <param name="settings">The typed per-environment settings to apply.</param>
    public void Update(ContainerAppEnvironmentSettingsData settings)
    {
        CpuCores = settings.CpuCores;
        MemoryGi = settings.MemoryGi;
        MinReplicas = settings.MinReplicas;
        MaxReplicas = settings.MaxReplicas;
        IngressEnabled = settings.IngressEnabled;
        IngressTargetPort = settings.IngressTargetPort;
        IngressExternal = settings.IngressExternal;
        TransportMethod = settings.TransportMethod;
        ReadinessProbePath = settings.ReadinessProbePath;
        ReadinessProbePort = settings.ReadinessProbePort;
        LivenessProbePath = settings.LivenessProbePath;
        LivenessProbePort = settings.LivenessProbePort;
        StartupProbePath = settings.StartupProbePath;
        StartupProbePort = settings.StartupProbePort;
        ContainerRegistryServiceConnection = settings.ContainerRegistryServiceConnection;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (CpuCores is not null) dict["cpuCores"] = CpuCores;
        if (MemoryGi is not null) dict["memoryGi"] = MemoryGi;
        if (MinReplicas is not null) dict["minReplicas"] = MinReplicas.Value.ToString();
        if (MaxReplicas is not null) dict["maxReplicas"] = MaxReplicas.Value.ToString();
        if (IngressEnabled is not null) dict["ingressEnabled"] = IngressEnabled.Value.ToString().ToLower();
        if (IngressTargetPort is not null) dict["ingressTargetPort"] = IngressTargetPort.Value.ToString();
        if (IngressExternal is not null) dict["ingressExternal"] = IngressExternal.Value.ToString().ToLower();
        if (TransportMethod is not null) dict["transportMethod"] = TransportMethod;
        if (ReadinessProbePath is not null) dict["readinessProbePath"] = ReadinessProbePath;
        if (ReadinessProbePort is not null) dict["readinessProbePort"] = ReadinessProbePort.Value.ToString();
        if (LivenessProbePath is not null) dict["livenessProbePath"] = LivenessProbePath;
        if (LivenessProbePort is not null) dict["livenessProbePort"] = LivenessProbePort.Value.ToString();
        if (StartupProbePath is not null) dict["startupProbePath"] = StartupProbePath;
        if (StartupProbePort is not null) dict["startupProbePort"] = StartupProbePort.Value.ToString();
        return dict;
    }
}
