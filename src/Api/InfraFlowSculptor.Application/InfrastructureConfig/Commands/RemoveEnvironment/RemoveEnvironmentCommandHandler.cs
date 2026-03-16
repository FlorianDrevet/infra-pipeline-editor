using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveEnvironment;

public class RemoveEnvironmentCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveEnvironmentCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveEnvironmentCommand command, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            repository, currentUser, command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithEnvironmentsAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        if (!infraConfig.RemoveEnvironment(command.EnvironmentId))
            return Errors.EnvironmentDefinition.NotFoundError(command.EnvironmentId);

        await repository.UpdateAsync(infraConfig);

        return Result.Deleted;
    }
}
