using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProject;

/// <summary>Handles the <see cref="CreateProjectCommand"/> request.</summary>
public sealed class CreateProjectCommandHandler(
    IProjectRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<CreateProjectCommand, ErrorOr<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var descriptionVo = command.Description is not null ? new Name(command.Description) : null;
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var project = Project.Create(nameVo, descriptionVo, userId);
        var saved = await repository.AddAsync(project);

        return mapper.Map<ProjectResult>(saved);
    }
}
