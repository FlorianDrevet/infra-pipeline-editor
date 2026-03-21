using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProject;

/// <summary>Command to create a new project.</summary>
/// <param name="Name">The name of the project.</param>
/// <param name="Description">Optional description of the project.</param>
public record CreateProjectCommand(string Name, string? Description) : IRequest<ErrorOr<ProjectResult>>;
