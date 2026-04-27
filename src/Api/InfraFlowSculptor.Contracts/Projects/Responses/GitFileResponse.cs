namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing a file found in a Git repository.</summary>
public record GitFileResponse(
    string Path,
    string Name);
