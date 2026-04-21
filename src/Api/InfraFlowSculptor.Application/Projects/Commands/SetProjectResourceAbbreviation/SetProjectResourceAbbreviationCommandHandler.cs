using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;

/// <summary>Handles the <see cref="SetProjectResourceAbbreviationCommand"/>.</summary>
public sealed class SetProjectResourceAbbreviationCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IMapper mapper)
    : ICommandHandler<SetProjectResourceAbbreviationCommand, ProjectResourceAbbreviationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResourceAbbreviationResult>> Handle(
        SetProjectResourceAbbreviationCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var entry = project.SetResourceAbbreviation(command.ResourceType, command.Abbreviation);

        await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectResourceAbbreviationResult>(entry);
    }
}
