using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;

/// <summary>Handles the <see cref="SetAgentPoolCommand"/>.</summary>
public sealed class SetAgentPoolCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<SetAgentPoolCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetAgentPoolCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        project.SetAgentPoolName(command.AgentPoolName);
        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }
}
