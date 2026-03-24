namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a push to Git operation.</summary>
public record PushBicepToGitResult(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount);
