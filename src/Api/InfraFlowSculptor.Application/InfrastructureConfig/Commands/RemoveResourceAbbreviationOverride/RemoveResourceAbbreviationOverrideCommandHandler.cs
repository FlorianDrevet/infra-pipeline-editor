using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;

/// <summary>Handles the <see cref="RemoveResourceAbbreviationOverrideCommand"/>.</summary>
public sealed class RemoveResourceAbbreviationOverrideCommandHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveResourceAbbreviationOverrideCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveResourceAbbreviationOverrideCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithNamingTemplatesAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        if (!infraConfig.RemoveResourceAbbreviationOverride(command.ResourceType))
            return Errors.InfrastructureConfig.ResourceAbbreviationOverrideNotFoundError(command.ResourceType);

        await repository.UpdateAsync(infraConfig);

        return Result.Deleted;
    }
}
