using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddConfigToProject;

/// <summary>Command to associate an infrastructure configuration with a project.</summary>
/// <param name="ProjectId">The project to associate the config to.</param>
/// <param name="ConfigId">The infrastructure configuration to associate.</param>
public record AddConfigToProjectCommand(Guid ProjectId, Guid ConfigId)
    : IRequest<ErrorOr<ProjectResult>>;
