using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveConfigFromProject;

/// <summary>Command to remove an infrastructure configuration from a project.</summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="ConfigId">The configuration to dissociate.</param>
public record RemoveConfigFromProjectCommand(Guid ProjectId, Guid ConfigId)
    : IRequest<ErrorOr<Deleted>>;
