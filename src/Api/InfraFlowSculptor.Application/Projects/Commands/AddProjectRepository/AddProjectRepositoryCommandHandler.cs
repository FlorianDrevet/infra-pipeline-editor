using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Handles the <see cref="AddProjectRepositoryCommand"/>.</summary>
public sealed class AddProjectRepositoryCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<AddProjectRepositoryCommand, ProjectRepositoryId>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectRepositoryId>> Handle(
        AddProjectRepositoryCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        GitProviderType? providerType = null;
        if (!string.IsNullOrWhiteSpace(command.ProviderType))
        {
            if (!Enum.TryParse<GitProviderTypeEnum>(command.ProviderType, ignoreCase: true, out var providerTypeEnum))
                return Errors.GitRepository.InvalidProviderType(command.ProviderType);
            providerType = new GitProviderType(providerTypeEnum);
        }

        var aliasResult = RepositoryAlias.Create(command.Alias);
        if (aliasResult.IsError)
            return aliasResult.Errors;

        var contentKindsResult = ParseContentKinds(command.ContentKinds);
        if (contentKindsResult.IsError)
            return contentKindsResult.Errors;

        var addResult = project.AddRepository(
            aliasResult.Value,
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKindsResult.Value);
        if (addResult.IsError)
            return addResult.Errors;

        await projectRepository.UpdateAsync(project);

        return addResult.Value.Id;
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        var flags = RepositoryContentKindsEnum.None;
        foreach (var raw in kinds)
        {
            if (!Enum.TryParse<RepositoryContentKindsEnum>(raw, ignoreCase: true, out var parsed)
                || parsed == RepositoryContentKindsEnum.None)
            {
                return Errors.ProjectRepository.NoContentKind();
            }

            flags |= parsed;
        }

        return RepositoryContentKinds.Create(flags);
    }
}
