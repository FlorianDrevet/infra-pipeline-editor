namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>Bootstrap file paths.</summary>
/// <param name="FilePaths">Union of all bootstrap files (with infra/app prefix in SplitInfraCode).</param>
/// <param name="InfraFilePaths">Infrastructure bootstrap files without the split prefix.</param>
/// <param name="AppFilePaths">Application bootstrap files without the split prefix.</param>
public record LatestBootstrapFiles(
    IReadOnlyDictionary<string, string> FilePaths,
    IReadOnlyDictionary<string, string> InfraFilePaths,
    IReadOnlyDictionary<string, string> AppFilePaths);