using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;

internal sealed record ContainerAppParameters
{
    [JsonPropertyName("containerImage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContainerImage { get; init; }

    [JsonPropertyName("containerRuntime")]
    public required ContainerRuntimeParameters ContainerRuntime { get; init; }

    [JsonPropertyName("scaling")]
    public required ScalingParameters Scaling { get; init; }

    [JsonPropertyName("ingress")]
    public required IngressParameters Ingress { get; init; }

    [JsonPropertyName("healthProbes")]
    public required HealthProbesParameters HealthProbes { get; init; }

    [JsonPropertyName("acrLoginServer")]
    public string? AcrLoginServer { get; init; }

    [JsonPropertyName("customDomains")]
    public IReadOnlyList<CustomDomainParameter>? CustomDomains { get; init; }
}
