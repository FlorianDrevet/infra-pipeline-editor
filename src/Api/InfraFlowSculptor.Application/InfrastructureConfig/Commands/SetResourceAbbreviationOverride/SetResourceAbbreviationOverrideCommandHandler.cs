using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;

/// <summary>Handles the <see cref="SetResourceAbbreviationOverrideCommand"/>.</summary>
public sealed class SetResourceAbbreviationOverrideCommandHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<SetResourceAbbreviationOverrideCommand, ResourceAbbreviationOverrideResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ResourceAbbreviationOverrideResult>> Handle(
        SetResourceAbbreviationOverrideCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithNamingTemplatesAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var entry = infraConfig.SetResourceAbbreviationOverride(command.ResourceType, command.Abbreviation);

        await repository.UpdateAsync(infraConfig);

        return mapper.Map<ResourceAbbreviationOverrideResult>(entry);
    }
}
