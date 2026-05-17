using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record HealthProbesParameters
{
    [JsonPropertyName("readiness")]
    public required HealthProbeParameters Readiness { get; init; }

    [JsonPropertyName("liveness")]
    public required HealthProbeParameters Liveness { get; init; }

    [JsonPropertyName("startup")]
    public required HealthProbeParameters Startup { get; init; }
}
