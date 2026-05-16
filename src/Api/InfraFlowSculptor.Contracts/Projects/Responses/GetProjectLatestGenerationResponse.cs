namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>
/// Response returned by the latest-generation endpoint.
/// Null sections indicate no generation exists for that artifact type (first-time or expired).
/// </summary>
/// <param name="Bicep">Latest Bicep generation file paths, or null.</param>
/// <param name="Pipeline">Latest Pipeline generation file paths, or null.</param>
/// <param name="Bootstrap">Latest Bootstrap generation file paths, or null.</param>
/// <param name="GeneratedAt">Timestamp string of the latest generation, or null if no generation exists.</param>
public record GetProjectLatestGenerationResponse(
    LatestBicepGenerationResponse? Bicep,
    LatestPipelineGenerationResponse? Pipeline,
    LatestBootstrapGenerationResponse? Bootstrap,
    string? GeneratedAt);

/// <summary>Bicep file paths from the latest generation.</summary>
public record LatestBicepGenerationResponse(
    IReadOnlyDictionary<string, string> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFileUris);

/// <summary>Pipeline file paths from the latest generation.</summary>
public record LatestPipelineGenerationResponse(
    IReadOnlyDictionary<string, string> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFileUris,
    IReadOnlyDictionary<string, string> InfraCommonFileUris,
    IReadOnlyDictionary<string, string> AppCommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> InfraConfigFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AppConfigFileUris);

/// <summary>Bootstrap file paths from the latest generation.</summary>
public record LatestBootstrapGenerationResponse(
    IReadOnlyDictionary<string, string> FileUris,
    IReadOnlyDictionary<string, string> InfraFileUris,
    IReadOnlyDictionary<string, string> AppFileUris);
