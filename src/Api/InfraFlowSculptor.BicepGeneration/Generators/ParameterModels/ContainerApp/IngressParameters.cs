using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record IngressParameters
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("targetPort")]
    public int TargetPort { get; init; }

    [JsonPropertyName("external")]
    public bool External { get; init; }

    [JsonPropertyName("transportMethod")]
    public required string TransportMethod { get; init; }
}
