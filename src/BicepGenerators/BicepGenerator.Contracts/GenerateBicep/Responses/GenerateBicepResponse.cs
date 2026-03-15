namespace BicepGenerator.Contracts.GenerateBicep.Responses;

public record GenerateBicepResponse(
    Uri MainBicepUri,
    Uri ParametersUri,
    IReadOnlyDictionary<string, Uri> ModuleUris);
