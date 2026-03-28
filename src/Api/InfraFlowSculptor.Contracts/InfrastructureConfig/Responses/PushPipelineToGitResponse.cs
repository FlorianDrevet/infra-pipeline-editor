namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response for a pipeline push to Git operation.</summary>
public record PushPipelineToGitResponse(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount);
