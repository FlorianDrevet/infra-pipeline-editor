using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;

/// <summary>Handles <see cref="UpdateInfraConfigRepositoryCommand"/>.</summary>
public sealed class UpdateInfraConfigRepositoryCommandHandler(
    IInfrastructureConfigRepository repo,
    IProjectAccessService accessService)
    : IRequestHandler<UpdateInfraConfigRepositoryCommand, ErrorOr<Updated>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(UpdateInfraConfigRepositoryCommand command, CancellationToken cancellationToken)
    {
        var auth = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (auth.IsError) return auth.Errors;

        var config = await repo.GetByIdAsync(command.ConfigId);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);
        if (config.ProjectId != command.ProjectId) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        if (!Enum.TryParse<GitProviderTypeEnum>(command.ProviderType, ignoreCase: true, out var providerTypeEnum))
            return Errors.GitRepository.InvalidProviderType(command.ProviderType);

        var flags = RepositoryContentKindsEnum.None;
        foreach (var raw in command.ContentKinds)
        {
            if (!Enum.TryParse<RepositoryContentKindsEnum>(raw, ignoreCase: true, out var parsed)
                || parsed == RepositoryContentKindsEnum.None)
            {
                return Errors.ProjectRepository.NoContentKind();
            }
            flags |= parsed;
        }
        var contentKinds = RepositoryContentKinds.Create(flags);
        if (contentKinds.IsError) return contentKinds.Errors;

        var updated = config.UpdateRepository(
            command.RepositoryId,
            new GitProviderType(providerTypeEnum),
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKinds.Value);
        if (updated.IsError) return updated.Errors;

        await repo.UpdateAsync(config);
        return Result.Updated;
    }
}
