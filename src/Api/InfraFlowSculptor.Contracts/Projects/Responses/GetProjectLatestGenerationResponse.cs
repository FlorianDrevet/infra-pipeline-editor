namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>
/// Response returned by the latest-generation endpoint.
/// Null sections indicate no generation exists for that artifact type (first-time or expired).
/// </summary>
/// <param name="Bicep">Latest Bicep generation file paths, or null.</param>
/// <param name="Pipeline">Latest Pipeline generation file paths, or null.</param>
/// <param name="Bootstrap">Latest Bootstrap generation file paths, or null.</param>
/// <param name="GeneratedAt">Timestamp folder name of the latest generation in <c>yyyyMMddHHmmss</c> format, or null if no generation exists.</param>
public record GetProjectLatestGenerationResponse(
    LatestBicepGenerationResponse? Bicep,
    LatestPipelineGenerationResponse? Pipeline,
    LatestBootstrapGenerationResponse? Bootstrap,
    string? GeneratedAt);
