using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>
/// Returns file listings of the latest generated artifacts (Bicep, Pipeline, Bootstrap) for a project
/// without re-generating. Returns null sections when no generation exists for that artifact type.
/// </summary>
public record GetProjectLatestGenerationQuery(
    Guid ProjectId
) : IQuery<GetProjectLatestGenerationResult>;

/// <summary>Result of the latest generation file listing.</summary>
/// <param name="Bicep">Latest Bicep generation file paths, or null if no Bicep generation exists.</param>
/// <param name="Pipeline">Latest Pipeline generation file paths, or null if no pipeline generation exists.</param>
/// <param name="Bootstrap">Latest Bootstrap generation file paths, or null if no bootstrap generation exists.</param>
/// <param name="GeneratedAt">ISO 8601 timestamp of the latest generation (uses bicep timestamp, then pipeline, then bootstrap).</param>
public record GetProjectLatestGenerationResult(
    LatestBicepFiles? Bicep,
    LatestPipelineFiles? Pipeline,
    LatestBootstrapFiles? Bootstrap,
    string? GeneratedAt);

/// <summary>Bicep file paths grouped by common and per-config.</summary>
/// <param name="CommonFilePaths">Paths under Common/ (types.bicep, functions.bicep, modules/...).</param>
/// <param name="ConfigFilePaths">Per-configuration file paths keyed by config name.</param>
public record LatestBicepFiles(
    IReadOnlyDictionary<string, string> CommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFilePaths);

/// <summary>Pipeline file paths grouped by common and per-config, with infra/app split.</summary>
public record LatestPipelineFiles(
    IReadOnlyDictionary<string, string> CommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFilePaths,
    IReadOnlyDictionary<string, string> InfraCommonFilePaths,
    IReadOnlyDictionary<string, string> AppCommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> InfraConfigFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> AppConfigFilePaths);

/// <summary>Bootstrap file paths.</summary>
/// <param name="FilePaths">Union of all bootstrap files (with infra/app prefix in SplitInfraCode).</param>
/// <param name="InfraFilePaths">Infrastructure bootstrap files (without prefix).</param>
/// <param name="AppFilePaths">Application bootstrap files (without prefix, SplitInfraCode only).</param>
public record LatestBootstrapFiles(
    IReadOnlyDictionary<string, string> FilePaths,
    IReadOnlyDictionary<string, string> InfraFilePaths,
    IReadOnlyDictionary<string, string> AppFilePaths);
