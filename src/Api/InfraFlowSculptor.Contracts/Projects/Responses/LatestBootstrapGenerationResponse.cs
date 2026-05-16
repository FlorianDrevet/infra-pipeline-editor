namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Bootstrap file paths from the latest generation.</summary>
/// <param name="FileUris">Union of all latest bootstrap file paths.</param>
/// <param name="InfraFileUris">Latest infrastructure bootstrap file paths.</param>
/// <param name="AppFileUris">Latest application bootstrap file paths.</param>
public record LatestBootstrapGenerationResponse(
    IReadOnlyDictionary<string, string> FileUris,
    IReadOnlyDictionary<string, string> InfraFileUris,
    IReadOnlyDictionary<string, string> AppFileUris);