using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
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

        var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
            command.ProviderType,
            static parsed => new GitProviderType(parsed),
            Errors.GitRepository.InvalidProviderType);
        if (providerTypeResult.IsError) return providerTypeResult.Errors;

        var contentKinds = RepositoryContentKindsParser.Parse(command.ContentKinds);
        if (contentKinds.IsError) return contentKinds.Errors;

        var updated = config.UpdateRepository(
            command.RepositoryId,
            providerTypeResult.Value,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKinds.Value);
        if (updated.IsError) return updated.Errors;

        repo.Update(config);
        return Result.Updated;
    }
}
