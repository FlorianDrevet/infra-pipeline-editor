namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for infrastructure-configuration-level bootstrap pipeline generation.</summary>
/// <param name="FileUris">Map of relative file paths to their blob URIs.</param>
public record GenerateBootstrapResponse(
    IReadOnlyDictionary<string, Uri> FileUris);
