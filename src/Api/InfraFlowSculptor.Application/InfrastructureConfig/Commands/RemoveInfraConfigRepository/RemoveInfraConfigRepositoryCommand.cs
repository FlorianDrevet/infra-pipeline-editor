using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveInfraConfigRepository;

/// <summary>Removes an InfraConfigRepository.</summary>
public sealed record RemoveInfraConfigRepositoryCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    InfraConfigRepositoryId RepositoryId) : IRequest<ErrorOr<Deleted>>;

/// <summary>Handles <see cref="RemoveInfraConfigRepositoryCommand"/>.</summary>
public sealed class RemoveInfraConfigRepositoryCommandHandler(
    IInfrastructureConfigRepository repo,
    IProjectAccessService accessService)
    : IRequestHandler<RemoveInfraConfigRepositoryCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(RemoveInfraConfigRepositoryCommand command, CancellationToken cancellationToken)
    {
        var auth = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (auth.IsError) return auth.Errors;

        var config = await repo.GetByIdAsync(command.ConfigId);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);
        if (config.ProjectId != command.ProjectId) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        var removed = config.RemoveRepository(command.RepositoryId);
        if (removed.IsError) return removed.Errors;

        await repo.UpdateAsync(config);
        return Result.Deleted;
    }
}
