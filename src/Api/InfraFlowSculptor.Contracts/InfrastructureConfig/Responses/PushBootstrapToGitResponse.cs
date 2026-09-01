namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for a bootstrap push to Git operation.</summary>
public record PushBootstrapToGitResponse(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount);
