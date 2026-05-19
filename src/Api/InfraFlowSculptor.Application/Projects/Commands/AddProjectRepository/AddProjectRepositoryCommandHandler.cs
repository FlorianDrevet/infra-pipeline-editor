using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Handles the <see cref="AddProjectRepositoryCommand"/>.</summary>
public sealed class AddProjectRepositoryCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
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

        var contentKindsResult = ParseContentKinds(command.ContentKinds);
        if (contentKindsResult.IsError)
            return contentKindsResult.Errors;

        var hasCompleteConnectionDetails = ProjectRepositoryConnectionVerifier.HasCompleteConnectionDetails(
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch);
        if (hasCompleteConnectionDetails)
        {
            if (string.IsNullOrWhiteSpace(command.PersonalAccessToken))
                return Errors.ProjectRepository.PersonalAccessTokenRequired();

            var verificationResult = await ProjectRepositoryConnectionVerifier.VerifyBranchesAsync(
                gitProviderFactory,
                providerType!,
                command.RepositoryUrl!,
                command.PersonalAccessToken,
                command.DefaultBranch,
                cancellationToken);
            if (verificationResult.IsError)
                return verificationResult.Errors;
        }

        var addResult = project.AddRepository(
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKindsResult.Value);
        if (addResult.IsError)
            return addResult.Errors;

        if (hasCompleteConnectionDetails)
        {
            var secretResult = await keyVaultSecretClient.SetSecretAsync(
                ProjectGitSecretNames.GetRepositoryPatSecretName(addResult.Value.Id),
                command.PersonalAccessToken!,
                cancellationToken);
            if (secretResult.IsError)
                return secretResult.Errors;
        }

        projectRepository.Update(project);

        return addResult.Value.Id;
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        return RepositoryContentKindsParser.Parse(kinds);
    }
}
