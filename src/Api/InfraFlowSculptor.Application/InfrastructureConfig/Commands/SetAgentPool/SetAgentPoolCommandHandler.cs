using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetAgentPool;

/// <summary>Handles the <see cref="SetAgentPoolCommand"/>.</summary>
public sealed class SetAgentPoolCommandHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<SetAgentPoolCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetAgentPoolCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        infraConfig.SetAgentPoolName(command.AgentPoolName);

        await repository.UpdateAsync(infraConfig);

        return Result.Success;
    }
}
