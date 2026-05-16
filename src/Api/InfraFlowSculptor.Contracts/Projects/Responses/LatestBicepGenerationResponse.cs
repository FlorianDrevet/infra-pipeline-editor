namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Bicep file paths from the latest generation.</summary>
/// <param name="CommonFileUris">Latest shared Bicep file paths.</param>
/// <param name="ConfigFileUris">Latest per-configuration Bicep file paths.</param>
public record LatestBicepGenerationResponse(
    IReadOnlyDictionary<string, string> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFileUris);