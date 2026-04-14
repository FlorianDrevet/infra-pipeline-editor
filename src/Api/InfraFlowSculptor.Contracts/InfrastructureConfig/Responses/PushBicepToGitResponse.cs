namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for a push to Git operation.</summary>
public record PushBicepToGitResponse(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount);
