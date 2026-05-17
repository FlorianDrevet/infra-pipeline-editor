using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
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
            var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
                command.ProviderType,
                static parsed => new GitProviderType(parsed),
                Errors.GitRepository.InvalidProviderType);
            if (providerTypeResult.IsError)
                return providerTypeResult.Errors;

            providerType = providerTypeResult.Value;
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

        projectRepository.Update(project);

        return addResult.Value.Id;
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        return RepositoryContentKindsParser.Parse(kinds);
    }
}
