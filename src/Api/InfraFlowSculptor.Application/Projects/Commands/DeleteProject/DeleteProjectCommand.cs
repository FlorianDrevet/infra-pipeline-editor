using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DeleteProject;

/// <summary>Command to delete a project. Requires Owner access.</summary>
public record DeleteProjectCommand(ProjectId ProjectId) : IRequest<ErrorOr<Unit>>;
