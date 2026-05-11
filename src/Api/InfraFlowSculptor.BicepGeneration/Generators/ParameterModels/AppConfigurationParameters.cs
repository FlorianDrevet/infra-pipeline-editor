using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record AppConfigurationParameters
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }
}