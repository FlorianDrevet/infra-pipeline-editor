namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for infrastructure-configuration-level bootstrap pipeline generation.</summary>
/// <param name="FileUris">Union of generated file paths to their blob URIs.</param>
/// <param name="InfraFileUris">Bootstrap files targeting the infrastructure repository.</param>
/// <param name="AppFileUris">Bootstrap files targeting the application repository.</param>
public record GenerateBootstrapResponse(
    IReadOnlyDictionary<string, Uri> FileUris,
    IReadOnlyDictionary<string, Uri> InfraFileUris,
    IReadOnlyDictionary<string, Uri> AppFileUris);
