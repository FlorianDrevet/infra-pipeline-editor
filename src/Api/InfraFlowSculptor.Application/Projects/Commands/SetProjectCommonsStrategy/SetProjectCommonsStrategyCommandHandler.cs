using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectCommonsStrategy;

/// <summary>Handles the <see cref="SetProjectCommonsStrategyCommand"/>.</summary>
public sealed class SetProjectCommonsStrategyCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<SetProjectCommonsStrategyCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetProjectCommonsStrategyCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (!Enum.TryParse<CommonsStrategyEnum>(command.Strategy, ignoreCase: true, out var parsed))
            return Errors.Project.InvalidCommonsStrategy(command.Strategy);

        var setResult = project.SetCommonsStrategy(new CommonsStrategy(parsed));
        if (setResult.IsError)
            return setResult.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }
}
