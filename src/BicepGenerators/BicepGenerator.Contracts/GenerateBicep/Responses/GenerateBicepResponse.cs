namespace BicepGenerator.Contracts.GenerateBicep.Responses;

/// <summary>Result of a Bicep generation request, containing URIs to the generated artifact files.</summary>
/// <param name="MainBicepUri">URI to the main <c>main.bicep</c> file that orchestrates all modules.</param>
/// <param name="ParametersUri">URI to the generated <c>main.bicepparam</c> parameters file.</param>
/// <param name="ModuleUris">Map of module names to their corresponding <c>.bicep</c> file URIs.</param>
public record GenerateBicepResponse(
    Uri MainBicepUri,
    Uri ParametersUri,
    IReadOnlyDictionary<string, Uri> ModuleUris);
