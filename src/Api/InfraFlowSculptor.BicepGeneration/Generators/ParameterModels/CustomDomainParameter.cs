using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record CustomDomainParameter
{
    [JsonPropertyName("domainName")]
    public required string DomainName { get; init; }

    [JsonPropertyName("bindingType")]
    public required string BindingType { get; init; }
}