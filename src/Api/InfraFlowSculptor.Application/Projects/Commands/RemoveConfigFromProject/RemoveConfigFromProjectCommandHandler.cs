using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveConfigFromProject;

/// <summary>Handles the <see cref="RemoveConfigFromProjectCommand"/> request.</summary>
public sealed class RemoveConfigFromProjectCommandHandler(
    IProjectAccessService accessService,
    IInfrastructureConfigRepository configRepository)
    : IRequestHandler<RemoveConfigFromProjectCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveConfigFromProjectCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);
        var accessResult = await accessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var configId = new InfrastructureConfigId(command.ConfigId);
        var config = await configRepository.GetByIdAsync(configId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        config.RemoveFromProject();
        await configRepository.UpdateAsync(config);

        return Result.Deleted;
    }
}
