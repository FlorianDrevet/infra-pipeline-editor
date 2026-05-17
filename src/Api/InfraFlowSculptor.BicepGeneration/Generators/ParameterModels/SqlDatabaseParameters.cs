using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record SqlDatabaseParameters
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("maxSizeBytes")]
    public long MaxSizeBytes { get; init; }

    [JsonPropertyName("collation")]
    public required string Collation { get; init; }

    [JsonPropertyName("zoneRedundant")]
    public bool ZoneRedundant { get; init; }
}
