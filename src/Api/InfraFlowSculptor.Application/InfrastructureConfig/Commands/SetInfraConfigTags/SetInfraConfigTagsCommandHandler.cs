using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;

/// <summary>Handles <see cref="SetInfraConfigTagsCommand"/> by replacing all configuration-level tags.</summary>
public sealed class SetInfraConfigTagsCommandHandler(
    IInfraConfigAccessService accessService)
    : ICommandHandler<SetInfraConfigTagsCommand, Updated>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(SetInfraConfigTagsCommand command, CancellationToken cancellationToken)
    {
        var infraConfigId = new InfrastructureConfigId(command.InfraConfigId);

        var accessResult = await accessService.VerifyWriteAccessAsync(infraConfigId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var config = accessResult.Value;

        var tags = command.Tags.Select(t => new Tag(t.Name, t.Value)).ToList();
        config.SetTags(tags);

        return Result.Updated;
    }
}
