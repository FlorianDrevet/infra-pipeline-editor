using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record WebAppParameters
{
    [JsonPropertyName("runtimeStack")]
    public required string RuntimeStack { get; init; }

    [JsonPropertyName("runtimeVersion")]
    public required string RuntimeVersion { get; init; }

    [JsonPropertyName("alwaysOn")]
    public bool AlwaysOn { get; init; }

    [JsonPropertyName("httpsOnly")]
    public bool HttpsOnly { get; init; }

    [JsonPropertyName("deploymentMode")]
    public required string DeploymentMode { get; init; }

    [JsonPropertyName("customDomains")]
    public IReadOnlyList<CustomDomainParameter> CustomDomains { get; init; } = [];

    [JsonPropertyName("dockerImageName")]
    public string? DockerImageName { get; init; }

    [JsonPropertyName("dockerImageTag")]
    public string? DockerImageTag { get; init; }

    [JsonPropertyName("acrLoginServer")]
    public string? AcrLoginServer { get; init; }

    [JsonPropertyName("acrUseManagedIdentityCreds")]
    public bool? AcrUseManagedIdentityCreds { get; init; }

    [JsonPropertyName("acrUserManagedIdentityId")]
    public string? AcrUserManagedIdentityId { get; init; }
}
