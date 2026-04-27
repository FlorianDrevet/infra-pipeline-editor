using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;

/// <summary>Handles <see cref="AddInfraConfigRepositoryCommand"/>.</summary>
public sealed class AddInfraConfigRepositoryCommandHandler(
    IInfrastructureConfigRepository repo,
    IProjectRepository projectRepo,
    IProjectAccessService accessService)
    : IRequestHandler<AddInfraConfigRepositoryCommand, ErrorOr<InfraConfigRepositoryId>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<InfraConfigRepositoryId>> Handle(AddInfraConfigRepositoryCommand command, CancellationToken cancellationToken)
    {
        var auth = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (auth.IsError) return auth.Errors;

        var project = await projectRepo.GetByIdAsync(command.ProjectId);
        if (project is null) return Errors.Project.NotFoundError(command.ProjectId);
        if (project.LayoutPreset.Value != LayoutPresetEnum.MultiRepo)
            return Errors.InfraConfigRepository.ProjectNotMultiRepo();

        var config = await repo.GetByIdAsync(command.ConfigId);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);
        if (config.ProjectId != command.ProjectId) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        if (!Enum.TryParse<GitProviderTypeEnum>(command.ProviderType, ignoreCase: true, out var providerTypeEnum))
            return Errors.GitRepository.InvalidProviderType(command.ProviderType);

        var aliasResult = RepositoryAlias.Create(command.Alias);
        if (aliasResult.IsError) return aliasResult.Errors;

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

        var added = config.AddRepository(
            aliasResult.Value,
            new GitProviderType(providerTypeEnum),
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKinds.Value);
        if (added.IsError) return added.Errors;

        await repo.UpdateAsync(config);
        return added.Value.Id;
    }
}
