using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;

/// <summary>Handles <see cref="SetInfraConfigLayoutModeCommand"/>.</summary>
public sealed class SetInfraConfigLayoutModeCommandHandler(
    IInfrastructureConfigRepository repo,
    IProjectAccessService accessService)
    : IRequestHandler<SetInfraConfigLayoutModeCommand, ErrorOr<Updated>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(SetInfraConfigLayoutModeCommand command, CancellationToken cancellationToken)
    {
        var auth = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (auth.IsError) return auth.Errors;

        var config = await repo.GetByIdAsync(command.ConfigId);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);
        if (config.ProjectId != command.ProjectId) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        ConfigLayoutMode? layout = null;
        if (!string.IsNullOrWhiteSpace(command.Mode))
        {
            var layoutModeResult = EnumValueObjectParser.Parse<ConfigLayoutModeEnum, ConfigLayoutMode>(
                command.Mode,
                static parsed => new ConfigLayoutMode(parsed),
                Errors.InfrastructureConfig.InvalidLayoutMode);
            if (layoutModeResult.IsError)
                return layoutModeResult.Errors;

            layout = layoutModeResult.Value;
        }

        config.SetLayoutMode(layout);
        repo.Update(config);
        return Result.Updated;
    }
}
