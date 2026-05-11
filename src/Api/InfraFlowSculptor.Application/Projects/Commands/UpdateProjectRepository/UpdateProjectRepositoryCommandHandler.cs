using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;

/// <summary>Handles the <see cref="UpdateProjectRepositoryCommand"/>.</summary>
public sealed class UpdateProjectRepositoryCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<UpdateProjectRepositoryCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        UpdateProjectRepositoryCommand command, CancellationToken cancellationToken)
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

        var contentKindsResult = ParseContentKinds(command.ContentKinds);
        if (contentKindsResult.IsError)
            return contentKindsResult.Errors;

        var updateResult = project.UpdateRepository(
            command.RepositoryId,
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKindsResult.Value);
        if (updateResult.IsError)
            return updateResult.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        return RepositoryContentKindsParser.Parse(kinds);
    }
}
