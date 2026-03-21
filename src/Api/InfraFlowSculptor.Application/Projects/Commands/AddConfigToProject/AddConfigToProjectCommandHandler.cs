using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddConfigToProject;

/// <summary>Handles the <see cref="AddConfigToProjectCommand"/> request.</summary>
public sealed class AddConfigToProjectCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    IMapper mapper)
    : IRequestHandler<AddConfigToProjectCommand, ErrorOr<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        AddConfigToProjectCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);
        var accessResult = await accessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var configId = new InfrastructureConfigId(command.ConfigId);
        var config = await configRepository.GetByIdAsync(configId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        if (config.ProjectId is not null)
            return Errors.Project.ConfigAlreadyAssignedError();

        config.AssignToProject(projectId);
        await configRepository.UpdateAsync(config);

        // Reload the project with configurations for the response
        var project = await projectRepository.GetByIdWithMembersAsync(projectId, cancellationToken);
        return mapper.Map<ProjectResult>(project!);
    }
}
