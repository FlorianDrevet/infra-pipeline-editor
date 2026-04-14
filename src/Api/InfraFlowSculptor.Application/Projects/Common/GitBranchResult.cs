namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result representing a Git branch.</summary>
public record GitBranchResult(
    string Name,
    bool IsProtected);
