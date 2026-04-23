using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a Git repository declared at project level. A project can declare
/// multiple repositories (multi-repo topology) and each
/// <see cref="InfrastructureConfigAggregate.InfrastructureConfig"/> binds to one of them
/// via its <c>RepositoryBinding</c>.
/// </summary>
public sealed class ProjectRepository : Entity<ProjectRepositoryId>
{
    /// <summary>Gets the parent project identifier.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Gets the project-scoped logical alias of this repository (slug).</summary>
    public RepositoryAlias Alias { get; private set; } = null!;

    /// <summary>Gets the Git hosting provider type.</summary>
    public GitProviderType ProviderType { get; private set; } = null!;

    /// <summary>Gets the full repository URL (e.g. https://github.com/org/repo).</summary>
    public string RepositoryUrl { get; private set; } = null!;

    /// <summary>Gets the repository owner (org or user, extracted from the URL).</summary>
    public string Owner { get; private set; } = null!;

    /// <summary>Gets the repository name (extracted from the URL).</summary>
    public string RepositoryName { get; private set; } = null!;

    /// <summary>Gets the default/base branch name (e.g. "main").</summary>
    public string DefaultBranch { get; private set; } = null!;

    /// <summary>Gets the kinds of content this repository hosts (infra, app code, pipelines).</summary>
    public RepositoryContentKinds ContentKinds { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private ProjectRepository() { }

    /// <summary>
    /// Creates a new <see cref="ProjectRepository"/> after validating its inputs and
    /// extracting the owner/repository from the URL.
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="alias">The project-scoped alias of the repository.</param>
    /// <param name="providerType">The Git hosting provider type.</param>
    /// <param name="repositoryUrl">The full repository URL.</param>
    /// <param name="defaultBranch">The default branch name.</param>
    /// <param name="contentKinds">The kinds of content hosted in the repository.</param>
    /// <returns>The created entity or a validation error.</returns>
    public static ErrorOr<ProjectRepository> Create(
        ProjectId projectId,
        RepositoryAlias alias,
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl ?? string.Empty);

        if (string.IsNullOrWhiteSpace(defaultBranch))
            return Errors.GitRepository.InvalidRepositoryUrl();

        (string owner, string repoName) parsed;
        try
        {
            parsed = RepositoryUrlHelper.ExtractOwnerAndRepo(providerType, repositoryUrl);
        }
        catch (UriFormatException)
        {
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl);
        }

        return new ProjectRepository
        {
            Id = ProjectRepositoryId.CreateUnique(),
            ProjectId = projectId,
            Alias = alias,
            ProviderType = providerType,
            RepositoryUrl = repositoryUrl.TrimEnd('/'),
            DefaultBranch = defaultBranch.Trim(),
            Owner = parsed.owner,
            RepositoryName = parsed.repoName,
            ContentKinds = contentKinds,
        };
    }

    /// <summary>Updates the mutable fields of this repository in place.</summary>
    /// <param name="providerType">The new Git hosting provider type.</param>
    /// <param name="repositoryUrl">The new repository URL.</param>
    /// <param name="defaultBranch">The new default branch name.</param>
    /// <param name="contentKinds">The new content kinds flags.</param>
    /// <returns><see cref="Result.Updated"/> on success, or a validation error.</returns>
    public ErrorOr<Updated> Update(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl ?? string.Empty);

        if (string.IsNullOrWhiteSpace(defaultBranch))
            return Errors.GitRepository.InvalidRepositoryUrl();

        (string owner, string repoName) parsed;
        try
        {
            parsed = RepositoryUrlHelper.ExtractOwnerAndRepo(providerType, repositoryUrl);
        }
        catch (UriFormatException)
        {
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl);
        }

        ProviderType = providerType;
        RepositoryUrl = repositoryUrl.TrimEnd('/');
        DefaultBranch = defaultBranch.Trim();
        Owner = parsed.owner;
        RepositoryName = parsed.repoName;
        ContentKinds = contentKinds;

        return Result.Updated;
    }
}
