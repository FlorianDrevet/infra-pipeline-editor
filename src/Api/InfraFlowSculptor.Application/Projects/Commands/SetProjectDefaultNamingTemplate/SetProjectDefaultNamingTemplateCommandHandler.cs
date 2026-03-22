using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;

/// <summary>Handles the <see cref="SetProjectDefaultNamingTemplateCommand"/>.</summary>
public sealed class SetProjectDefaultNamingTemplateCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : IRequestHandler<SetProjectDefaultNamingTemplateCommand, ErrorOr<Success>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetProjectDefaultNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var template = command.Template is not null ? new NamingTemplate(command.Template) : null;
        project.SetDefaultNamingTemplate(template);

        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }
}
