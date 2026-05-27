using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record AppServicePlanParameters
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; init; }

    [JsonPropertyName("osType")]
    public required string OsType { get; init; }
}
