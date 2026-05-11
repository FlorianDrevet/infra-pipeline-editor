using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record SqlServerParameters
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("administratorLogin")]
    public required string AdministratorLogin { get; init; }

    [JsonPropertyName("minimalTlsVersion")]
    public required string MinimalTlsVersion { get; init; }
}