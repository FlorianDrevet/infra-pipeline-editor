using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;

/// <summary>Handles the <see cref="RemoveProjectResourceNamingTemplateCommand"/>.</summary>
public sealed class RemoveProjectResourceNamingTemplateCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : IRequestHandler<RemoveProjectResourceNamingTemplateCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectResourceNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var removed = project.RemoveResourceNamingTemplate(command.ResourceType);
        if (!removed)
            return Errors.Project.NotFoundError(command.ProjectId);

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
