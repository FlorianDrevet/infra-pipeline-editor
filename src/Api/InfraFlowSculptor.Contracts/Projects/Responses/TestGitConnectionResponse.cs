namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response for a Git connection test.</summary>
public record TestGitConnectionResponse(
    bool Success,
    string? RepositoryFullName,
    string? DefaultBranch,
    string? ErrorMessage);
