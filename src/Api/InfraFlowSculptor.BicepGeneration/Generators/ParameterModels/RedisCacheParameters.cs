using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record RedisCacheParameters
{
    [JsonPropertyName("skuName")]
    public required string SkuName { get; init; }

    [JsonPropertyName("skuFamily")]
    public required string SkuFamily { get; init; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; init; }

    [JsonPropertyName("redisVersion")]
    public required string RedisVersion { get; init; }

    [JsonPropertyName("enableNonSslPort")]
    public bool EnableNonSslPort { get; init; }

    [JsonPropertyName("minimumTlsVersion")]
    public required string MinimumTlsVersion { get; init; }

    [JsonPropertyName("disableAccessKeyAuthentication")]
    public bool DisableAccessKeyAuthentication { get; init; }

    [JsonPropertyName("aadEnabled")]
    public bool AadEnabled { get; init; }
}