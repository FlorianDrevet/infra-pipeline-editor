using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Provides the Key Vault secret names used by repository-level Git features.
/// </summary>
internal static class ProjectGitSecretNames
{
    private const string RepositoryPatSecretPrefix = "git-pat-repo-";

    internal static string GetRepositoryPatSecretName(ProjectRepositoryId repositoryId)
    {
        return $"{RepositoryPatSecretPrefix}{repositoryId.Value}";
    }
}