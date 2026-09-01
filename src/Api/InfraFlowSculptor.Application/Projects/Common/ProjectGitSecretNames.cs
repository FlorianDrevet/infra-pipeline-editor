using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Provides the Key Vault secret names used by repository-level Git features.
/// </summary>
internal static class ProjectGitSecretNames
{
    private const string RepositoryPatSecretPrefix = "git-pat-repo-";

    /// <summary>Returns the Key Vault secret name storing the PAT of a project-level repository.</summary>
    /// <param name="repositoryId">Identifier of the project-level repository.</param>
    /// <returns>The Key Vault secret name.</returns>
    internal static string GetRepositoryPatSecretName(ProjectRepositoryId repositoryId)
    {
        return $"{RepositoryPatSecretPrefix}{repositoryId.Value}";
    }

    /// <summary>Returns the Key Vault secret name storing the PAT of a configuration-level repository.</summary>
    /// <param name="repositoryId">Identifier of the configuration-level repository.</param>
    /// <returns>The Key Vault secret name.</returns>
    /// <remarks>
    /// Uses the same <c>git-pat-repo-</c> prefix as <see cref="GetRepositoryPatSecretName(ProjectRepositoryId)"/>.
    /// <see cref="ProjectRepositoryId"/> and <see cref="InfraConfigRepositoryId"/> both wrap a globally unique
    /// <see cref="Guid"/>, so the two secret namespaces never collide.
    /// </remarks>
    internal static string GetInfraConfigRepositoryPatSecretName(InfraConfigRepositoryId repositoryId)
    {
        return $"{RepositoryPatSecretPrefix}{repositoryId.Value}";
    }
}