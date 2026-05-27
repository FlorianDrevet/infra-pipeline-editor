using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record ScalingParameters
{
    [JsonPropertyName("minReplicas")]
    public int MinReplicas { get; init; }

    [JsonPropertyName("maxReplicas")]
    public int MaxReplicas { get; init; }
}
