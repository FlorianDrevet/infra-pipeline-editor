using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;

/// <summary>Handles the <see cref="SetProjectResourceNamingTemplateCommand"/>.</summary>
public sealed class SetProjectResourceNamingTemplateCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IMapper mapper)
    : IRequestHandler<SetProjectResourceNamingTemplateCommand, ErrorOr<ProjectResourceNamingTemplateResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResourceNamingTemplateResult>> Handle(
        SetProjectResourceNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var entry = project.SetResourceNamingTemplate(command.ResourceType, new NamingTemplate(command.Template));

        await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectResourceNamingTemplateResult>(entry);
    }
}
