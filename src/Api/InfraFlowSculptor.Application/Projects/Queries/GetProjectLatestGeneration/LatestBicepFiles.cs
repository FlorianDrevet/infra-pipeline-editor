namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>Bicep file paths grouped by common and per-config.</summary>
/// <param name="CommonFilePaths">Paths under Common/ (types.bicep, functions.bicep, modules/...).</param>
/// <param name="ConfigFilePaths">Per-configuration file paths keyed by config name.</param>
public record LatestBicepFiles(
    IReadOnlyDictionary<string, string> CommonFilePaths,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFilePaths);
