using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;

/// <summary>Command to remove a project-level environment definition.</summary>
public record RemoveProjectEnvironmentCommand(
    ProjectId ProjectId,
    ProjectEnvironmentDefinitionId EnvironmentId
) : IRequest<ErrorOr<Deleted>>;
