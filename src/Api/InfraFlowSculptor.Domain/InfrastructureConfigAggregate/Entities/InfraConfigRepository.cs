using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Represents a Git repository declared at <see cref="InfrastructureConfig"/> level.
/// Only used when the parent project is in
/// <see cref="ProjectAggregate.ValueObjects.LayoutPresetEnum.MultiRepo"/> mode.
/// </summary>
public sealed class InfraConfigRepository : Entity<InfraConfigRepositoryId>
{
    /// <summary>Gets the parent infrastructure configuration identifier.</summary>
    public InfrastructureConfigId InfrastructureConfigId { get; private set; } = null!;

    /// <summary>Gets the configuration-scoped logical alias of this repository (slug).</summary>
    public RepositoryAlias Alias { get; private set; } = null!;

    /// <summary>Gets the Git hosting provider type.</summary>
    public GitProviderType ProviderType { get; private set; } = null!;

    /// <summary>Gets the full repository URL.</summary>
    public string RepositoryUrl { get; private set; } = null!;

    /// <summary>Gets the repository owner (org or user, extracted from the URL).</summary>
    public string Owner { get; private set; } = null!;

    /// <summary>Gets the repository name (extracted from the URL).</summary>
    public string RepositoryName { get; private set; } = null!;

    /// <summary>Gets the default branch name.</summary>
    public string DefaultBranch { get; private set; } = null!;

    /// <summary>Gets the kinds of content this repository hosts.</summary>
    public RepositoryContentKinds ContentKinds { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private InfraConfigRepository() { }

    /// <summary>Creates a new <see cref="InfraConfigRepository"/> after validation and URL parsing.</summary>
    public static ErrorOr<InfraConfigRepository> Create(
        InfrastructureConfigId configId,
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

        return new InfraConfigRepository
        {
            Id = InfraConfigRepositoryId.CreateUnique(),
            InfrastructureConfigId = configId,
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
