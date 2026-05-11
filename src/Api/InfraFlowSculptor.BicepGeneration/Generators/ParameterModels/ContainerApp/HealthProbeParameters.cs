using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record HealthProbeParameters
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("port")]
    public int Port { get; init; }
}