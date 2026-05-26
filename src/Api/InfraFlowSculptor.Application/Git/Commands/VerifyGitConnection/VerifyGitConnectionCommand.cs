using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Git.Commands.VerifyGitConnection;

/// <summary>
/// Stateless command that verifies a Git repository connection and returns available branches.
/// Does not require a project to exist — used by the wizard before project creation.
/// </summary>
/// <param name="ProviderType">Git hosting provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL to verify.</param>
/// <param name="PersonalAccessToken">Personal Access Token used to authenticate against the provider.</param>
public sealed record VerifyGitConnectionCommand(
    string ProviderType,
    string RepositoryUrl,
    string PersonalAccessToken)
    : ICommand<ProjectRepositoryConnectionVerificationResult>;
