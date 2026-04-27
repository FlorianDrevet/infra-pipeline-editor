using ErrorOr;
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
            if (!Enum.TryParse<ConfigLayoutModeEnum>(command.Mode, ignoreCase: true, out var parsed))
                return Error.Validation("InfraConfigRepository.InvalidLayoutMode", $"Invalid layout mode '{command.Mode}'. Valid values: AllInOne, SplitInfraCode.");
            layout = new ConfigLayoutMode(parsed);
        }

        config.SetLayoutMode(layout);
        await repo.UpdateAsync(config);
        return Result.Updated;
    }
}
