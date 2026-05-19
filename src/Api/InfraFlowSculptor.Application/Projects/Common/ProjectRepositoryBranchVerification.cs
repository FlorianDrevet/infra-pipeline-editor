namespace InfraFlowSculptor.Application.Projects.Common;

internal sealed record ProjectRepositoryBranchVerification(
    string Owner,
    string RepositoryName,
    IReadOnlyList<GitBranchResult> Branches);