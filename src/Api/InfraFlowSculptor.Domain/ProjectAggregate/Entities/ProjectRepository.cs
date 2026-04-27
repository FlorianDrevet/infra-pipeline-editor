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
/// <remarks>
/// A repository can be created in a "slot" state where only the <see cref="Alias"/> and
/// <see cref="ContentKinds"/> are known. The Git connection details
/// (<see cref="ProviderType"/>, <see cref="RepositoryUrl"/>, <see cref="DefaultBranch"/>)
/// can be filled in later via <see cref="Update"/>. Generation and push flows that need
/// connection details should check <see cref="IsConfigured"/> before using the repository.
/// </remarks>
public sealed class ProjectRepository : Entity<ProjectRepositoryId>
{
    /// <summary>Gets the parent project identifier.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Gets the project-scoped logical alias of this repository (slug).</summary>
    public RepositoryAlias Alias { get; private set; } = null!;

    /// <summary>Gets the Git hosting provider type, or <c>null</c> if not yet configured.</summary>
    public GitProviderType? ProviderType { get; private set; }

    /// <summary>Gets the full repository URL (e.g. https://github.com/org/repo), or <c>null</c> if not yet configured.</summary>
    public string? RepositoryUrl { get; private set; }

    /// <summary>Gets the repository owner extracted from the URL, or <c>null</c> if not yet configured.</summary>
    public string? Owner { get; private set; }

    /// <summary>Gets the repository name extracted from the URL, or <c>null</c> if not yet configured.</summary>
    public string? RepositoryName { get; private set; }

    /// <summary>Gets the default branch name (e.g. "main"), or <c>null</c> if not yet configured.</summary>
    public string? DefaultBranch { get; private set; }

    /// <summary>Gets the kinds of content this repository hosts (infra, app code, pipelines).</summary>
    public RepositoryContentKinds ContentKinds { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the Git connection details are fully configured.
    /// Returns <c>true</c> when provider, URL and default branch are all set.
    /// </summary>
    public bool IsConfigured =>
        ProviderType is not null
        && !string.IsNullOrWhiteSpace(RepositoryUrl)
        && !string.IsNullOrWhiteSpace(DefaultBranch);

    /// <summary>EF Core constructor.</summary>
    private ProjectRepository() { }

    /// <summary>
    /// Creates a new <see cref="ProjectRepository"/>. Connection details
    /// (<paramref name="providerType"/>, <paramref name="repositoryUrl"/>, <paramref name="defaultBranch"/>)
    /// are optional: pass them all to create a fully configured repository, or pass them all as
    /// <c>null</c>/empty to create an unconfigured slot to be completed later via <see cref="Update"/>.
    /// Partially-supplied connection details are rejected.
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="alias">The project-scoped alias of the repository.</param>
    /// <param name="providerType">The Git hosting provider type, or <c>null</c> for an unconfigured slot.</param>
    /// <param name="repositoryUrl">The full repository URL, or <c>null</c>/empty for an unconfigured slot.</param>
    /// <param name="defaultBranch">The default branch name, or <c>null</c>/empty for an unconfigured slot.</param>
    /// <param name="contentKinds">The kinds of content hosted in the repository.</param>
    /// <returns>The created entity or a validation error.</returns>
    public static ErrorOr<ProjectRepository> Create(
        ProjectId projectId,
        RepositoryAlias alias,
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        var repo = new ProjectRepository
        {
            Id = ProjectRepositoryId.CreateUnique(),
            ProjectId = projectId,
            Alias = alias,
            ContentKinds = contentKinds,
        };

        var applied = repo.ApplyConnectionDetails(providerType, repositoryUrl, defaultBranch);
        if (applied.IsError)
            return applied.Errors;

        return repo;
    }

    /// <summary>Updates the mutable fields of this repository in place.</summary>
    /// <param name="providerType">The Git hosting provider type, or <c>null</c> to clear the slot.</param>
    /// <param name="repositoryUrl">The repository URL, or <c>null</c>/empty to clear the slot.</param>
    /// <param name="defaultBranch">The default branch name, or <c>null</c>/empty to clear the slot.</param>
    /// <param name="contentKinds">The new content kinds flags.</param>
    /// <returns><see cref="Result.Updated"/> on success, or a validation error.</returns>
    public ErrorOr<Updated> Update(
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        var applied = ApplyConnectionDetails(providerType, repositoryUrl, defaultBranch);
        if (applied.IsError)
            return applied.Errors;

        ContentKinds = contentKinds;
        return Result.Updated;
    }

    private ErrorOr<Success> ApplyConnectionDetails(
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch)
    {
        var hasUrl = !string.IsNullOrWhiteSpace(repositoryUrl);
        var hasBranch = !string.IsNullOrWhiteSpace(defaultBranch);
        var hasProvider = providerType is not null;

        // Either fully unconfigured (slot) or fully configured. Partial configuration is rejected.
        if (!hasUrl && !hasBranch && !hasProvider)
        {
            ProviderType = null;
            RepositoryUrl = null;
            Owner = null;
            RepositoryName = null;
            DefaultBranch = null;
            return Result.Success;
        }

        if (!hasUrl || !hasBranch || !hasProvider)
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl ?? string.Empty);

        (string owner, string repoName) parsed;
        try
        {
            parsed = RepositoryUrlHelper.ExtractOwnerAndRepo(providerType!, repositoryUrl!);
        }
        catch (UriFormatException)
        {
            return Errors.ProjectRepository.InvalidUrl(repositoryUrl!);
        }

        ProviderType = providerType;
        RepositoryUrl = repositoryUrl!.TrimEnd('/');
        DefaultBranch = defaultBranch!.Trim();
        Owner = parsed.owner;
        RepositoryName = parsed.repoName;
        return Result.Success;
    }
}
