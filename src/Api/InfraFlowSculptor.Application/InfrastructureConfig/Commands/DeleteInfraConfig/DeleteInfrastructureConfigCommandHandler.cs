using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;

/// <summary>Handles the <see cref="DeleteInfrastructureConfigCommand"/>.</summary>
public sealed class DeleteInfrastructureConfigCommandHandler(
    IInfrastructureConfigRepository configRepository,
    IProjectAccessService projectAccessService)
    : ICommandHandler<DeleteInfrastructureConfigCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteInfrastructureConfigCommand command,
        CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByIdAsync(command.InfraConfigId, cancellationToken);
        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var accessResult = await projectAccessService.VerifyOwnerAccessAsync(
            config.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        await configRepository.DeleteAsync(command.InfraConfigId);

        return Result.Deleted;
    }
}
