using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record StorageAccountParameters
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("accessTier")]
    public required string AccessTier { get; init; }

    [JsonPropertyName("allowBlobPublicAccess")]
    public bool AllowBlobPublicAccess { get; init; }

    [JsonPropertyName("supportsHttpsTrafficOnly")]
    public bool SupportsHttpsTrafficOnly { get; init; }

    [JsonPropertyName("minimumTlsVersion")]
    public required string MinimumTlsVersion { get; init; }
}