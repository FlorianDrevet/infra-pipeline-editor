namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a file found in a Git repository.</summary>
public record GitFileResult(
    /// <summary>Full path from the repository root (e.g. "src/MyApp/Dockerfile").</summary>
    string Path,
    /// <summary>File name only (e.g. "Dockerfile").</summary>
    string Name);
