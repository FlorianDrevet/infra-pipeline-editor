using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;

/// <summary>Handles the <see cref="SetInheritanceCommand"/>.</summary>
public sealed class SetInheritanceCommandHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<SetInheritanceCommand, ErrorOr<Success>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetInheritanceCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        infraConfig.SetUseProjectNamingConventions(command.UseProjectNamingConventions);

        await repository.UpdateAsync(infraConfig);

        return Result.Success;
    }
}
