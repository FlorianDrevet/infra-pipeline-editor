using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;

/// <summary>Command to update an existing project-level Git repository declaration.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="RepositoryId">Identifier of the repository entity to update.</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="DefaultBranch">Default branch name.</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository.</param>
public record UpdateProjectRepositoryCommand(
    ProjectId ProjectId,
    ProjectRepositoryId RepositoryId,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds
) : ICommand<Success>;
