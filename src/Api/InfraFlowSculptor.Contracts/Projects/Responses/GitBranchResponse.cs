namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing a Git branch.</summary>
public record GitBranchResponse(
    string Name,
    bool IsProtected);
