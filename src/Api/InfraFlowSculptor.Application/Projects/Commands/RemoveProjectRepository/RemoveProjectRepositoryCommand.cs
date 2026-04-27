using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;

/// <summary>Command to remove a project-level Git repository declaration.</summary>
/// <param name="ProjectId">Identifier of the parent project.</param>
/// <param name="RepositoryId">Identifier of the repository entity to remove.</param>
public record RemoveProjectRepositoryCommand(
    ProjectId ProjectId,
    ProjectRepositoryId RepositoryId
) : ICommand<Deleted>;
