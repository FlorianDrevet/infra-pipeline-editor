using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Command to add a new project-level Git repository declaration.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>),
/// or <c>null</c> to create an unconfigured slot.</param>
/// <param name="RepositoryUrl">Full repository URL, or <c>null</c>/empty to create an unconfigured slot.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>),
/// or <c>null</c>/empty to create an unconfigured slot.</param>
/// <param name="PersonalAccessToken">Transient PAT used to verify and save configured repositories.</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository
/// (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>).</param>
public record AddProjectRepositoryCommand(
    ProjectId ProjectId,
    string? ProviderType,
    string? RepositoryUrl,
    string? DefaultBranch,
    string? PersonalAccessToken,
    IReadOnlyList<string> ContentKinds
) : ICommand<ProjectRepositoryId>;
