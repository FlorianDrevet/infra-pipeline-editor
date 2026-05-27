using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;

/// <summary>Handles the <see cref="UpdateProjectRepositoryCommand"/>.</summary>
public sealed class UpdateProjectRepositoryCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
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

        if (!project.Repositories.Any(repository => repository.Id == command.RepositoryId))
            return Errors.ProjectRepository.NotFound(command.RepositoryId);

        var providerTypeResult = ParseProviderType(command.ProviderType);
        if (providerTypeResult.IsError)
            return providerTypeResult.Errors;

        var providerType = providerTypeResult.Value;

        var contentKindsResult = ParseContentKinds(command.ContentKinds);
        if (contentKindsResult.IsError)
            return contentKindsResult.Errors;

        var shouldStorePersonalAccessTokenResult = await VerifyConnectionAsync(command, providerType, cancellationToken)
            .ConfigureAwait(false);
        if (shouldStorePersonalAccessTokenResult.IsError)
            return shouldStorePersonalAccessTokenResult.Errors;

        var updateResult = project.UpdateRepository(
            command.RepositoryId,
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKindsResult.Value);
        if (updateResult.IsError)
            return updateResult.Errors;

        if (shouldStorePersonalAccessTokenResult.Value)
        {
            var secretResult = await keyVaultSecretClient.SetSecretAsync(
                ProjectGitSecretNames.GetRepositoryPatSecretName(command.RepositoryId),
                command.PersonalAccessToken!,
                cancellationToken);
            if (secretResult.IsError)
                return secretResult.Errors;
        }

        projectRepository.Update(project);

        return Result.Success;
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        return RepositoryContentKindsParser.Parse(kinds);
    }

    private static ErrorOr<GitProviderType?> ParseProviderType(string? providerType)
    {
        if (string.IsNullOrWhiteSpace(providerType))
            return (GitProviderType?)null;

        var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
            providerType,
            static parsed => new GitProviderType(parsed),
            Errors.GitRepository.InvalidProviderType);
        if (providerTypeResult.IsError)
            return providerTypeResult.Errors;

        return providerTypeResult.Value;
    }

    private async Task<ErrorOr<bool>> VerifyConnectionAsync(
        UpdateProjectRepositoryCommand command,
        GitProviderType? providerType,
        CancellationToken cancellationToken)
    {
        if (!ProjectRepositoryConnectionVerifier.HasCompleteConnectionDetails(
                providerType,
                command.RepositoryUrl,
                command.DefaultBranch))
        {
            return false;
        }

        var personalAccessTokenResult = await GetVerificationPersonalAccessTokenAsync(command, cancellationToken)
            .ConfigureAwait(false);
        if (personalAccessTokenResult.IsError)
            return personalAccessTokenResult.Errors;

        var verificationResult = await ProjectRepositoryConnectionVerifier.VerifyBranchesAsync(
            gitProviderFactory,
            providerType!,
            command.RepositoryUrl!,
            personalAccessTokenResult.Value,
            command.DefaultBranch,
            cancellationToken);
        if (verificationResult.IsError)
            return verificationResult.Errors;

        return !string.IsNullOrWhiteSpace(command.PersonalAccessToken);
    }

    private async Task<ErrorOr<string>> GetVerificationPersonalAccessTokenAsync(
        UpdateProjectRepositoryCommand command,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.PersonalAccessToken))
            return command.PersonalAccessToken;

        return await keyVaultSecretClient.GetSecretAsync(
                ProjectGitSecretNames.GetRepositoryPatSecretName(command.RepositoryId),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
