namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Result of a Bicep generation request, containing URIs to the generated artifact files.</summary>
/// <param name="MainBicepUri">URI to the main <c>main.bicep</c> file that orchestrates all modules.</param>
/// <param name="ConstantsBicepUri">URI to the optional <c>constants.bicep</c> file (present only when role assignments are configured).</param>
/// <param name="ParameterFileUris">Map of parameter file names (e.g. <c>main.dev.bicepparam</c>) to their URIs.</param>
/// <param name="ModuleUris">Map of module names to their corresponding <c>.bicep</c> file URIs.</param>
public record GenerateBicepResponse(
    Uri MainBicepUri,
    Uri? ConstantsBicepUri,
    IReadOnlyDictionary<string, Uri> ParameterFileUris,
    IReadOnlyDictionary<string, Uri> ModuleUris);
