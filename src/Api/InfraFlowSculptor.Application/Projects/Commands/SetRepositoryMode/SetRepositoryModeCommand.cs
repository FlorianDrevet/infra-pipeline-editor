using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetRepositoryMode;

/// <summary>Command to set the repository mode (MonoRepo/MultiRepo) on a project.</summary>
public record SetRepositoryModeCommand(
    ProjectId ProjectId,
    string RepositoryMode
) : IRequest<ErrorOr<Success>>;
