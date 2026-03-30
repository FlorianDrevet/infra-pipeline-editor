using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.DeleteProject;

/// <summary>Command to delete a project. Requires Owner access.</summary>
public record DeleteProjectCommand(ProjectId ProjectId) : ICommand<Unit>;
