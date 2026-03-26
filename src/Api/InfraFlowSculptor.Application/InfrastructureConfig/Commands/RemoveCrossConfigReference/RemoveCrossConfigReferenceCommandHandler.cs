using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;

/// <summary>Handles removing a cross-configuration resource reference.</summary>
public sealed class RemoveCrossConfigReferenceCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : IRequestHandler<RemoveCrossConfigReferenceCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveCrossConfigReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfraConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = authResult.Value;

        var referenceId = new CrossConfigResourceReferenceId(command.ReferenceId);
        var result = config.RemoveCrossConfigReference(referenceId);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);

        return Result.Deleted;
    }
}
