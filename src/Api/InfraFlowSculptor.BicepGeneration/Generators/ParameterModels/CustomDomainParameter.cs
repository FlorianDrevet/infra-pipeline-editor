using System.Text.Json.Serialization;

namespace InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;

internal sealed record CustomDomainParameter
{
    [JsonPropertyName("domainName")]
    public required string DomainName { get; init; }

    [JsonPropertyName("bindingType")]
    public required string BindingType { get; init; }

    [JsonPropertyName("certificateMode")]
    public required string CertificateMode { get; init; }

    [JsonPropertyName("keyVaultUrl")]
    public string? KeyVaultUrl { get; init; }

    [JsonPropertyName("managedIdentityResourceId")]
    public string? ManagedIdentityResourceId { get; init; }

    [JsonPropertyName("certificateName")]
    public string? CertificateName { get; init; }
}
