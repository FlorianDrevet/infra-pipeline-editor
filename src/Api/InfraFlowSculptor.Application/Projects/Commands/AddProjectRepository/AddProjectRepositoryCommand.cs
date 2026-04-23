using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Command to add a new project-level Git repository declaration.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="Alias">Project-scoped slug-like alias (lowercase letters, digits and hyphens).</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>).</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository
/// (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>, <c>Pipelines</c>).</param>
public record AddProjectRepositoryCommand(
    ProjectId ProjectId,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds
) : ICommand<ProjectRepositoryId>;
