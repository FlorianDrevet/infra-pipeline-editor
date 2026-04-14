namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a Git connection test.</summary>
public record TestGitConnectionResult(
    bool Success,
    string? RepositoryFullName,
    string? DefaultBranch,
    string? ErrorMessage);
