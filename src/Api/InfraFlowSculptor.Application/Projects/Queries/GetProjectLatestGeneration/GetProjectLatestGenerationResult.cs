namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>Result of the latest generation file listing.</summary>
/// <param name="Bicep">Latest Bicep generation file paths, or null if no Bicep generation exists.</param>
/// <param name="Pipeline">Latest Pipeline generation file paths, or null if no pipeline generation exists.</param>
/// <param name="Bootstrap">Latest Bootstrap generation file paths, or null if no bootstrap generation exists.</param>
/// <param name="GeneratedAt">Timestamp folder name of the latest generation in <c>yyyyMMddHHmmss</c> format.</param>
public record GetProjectLatestGenerationResult(
    LatestBicepFiles? Bicep,
    LatestPipelineFiles? Pipeline,
    LatestBootstrapFiles? Bootstrap,
    string? GeneratedAt);
