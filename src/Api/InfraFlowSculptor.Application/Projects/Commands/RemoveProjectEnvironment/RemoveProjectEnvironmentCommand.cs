using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;

/// <summary>Command to remove a project-level environment definition.</summary>
public record RemoveProjectEnvironmentCommand(
    ProjectId ProjectId,
    ProjectEnvironmentDefinitionId EnvironmentId
) : ICommand<Deleted>;
