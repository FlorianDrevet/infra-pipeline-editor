using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;

/// <summary>Handles <see cref="UpdateInfraConfigRepositoryCommand"/>.</summary>
public sealed class UpdateInfraConfigRepositoryCommandHandler(
    IInfrastructureConfigRepository repo,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
    : IRequestHandler<UpdateInfraConfigRepositoryCommand, ErrorOr<Updated>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(UpdateInfraConfigRepositoryCommand command, CancellationToken cancellationToken)
    {
        var auth = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (auth.IsError) return auth.Errors;

        var config = await repo.GetByIdAsync(command.ConfigId, cancellationToken);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);
        if (config.ProjectId != command.ProjectId) return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
            command.ProviderType,
            static parsed => new GitProviderType(parsed),
            Errors.GitRepository.InvalidProviderType);
        if (providerTypeResult.IsError) return providerTypeResult.Errors;

        var providerType = providerTypeResult.Value;

        var contentKinds = RepositoryContentKindsParser.Parse(command.ContentKinds);
        if (contentKinds.IsError) return contentKinds.Errors;

        var shouldStorePersonalAccessTokenResult = await VerifyConnectionAsync(command, providerType, cancellationToken)
            .ConfigureAwait(false);
        if (shouldStorePersonalAccessTokenResult.IsError) return shouldStorePersonalAccessTokenResult.Errors;

        var updated = config.UpdateRepository(
            command.RepositoryId,
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            contentKinds.Value);
        if (updated.IsError) return updated.Errors;

        if (shouldStorePersonalAccessTokenResult.Value)
        {
            var secretResult = await keyVaultSecretClient.SetSecretAsync(
                ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(command.RepositoryId),
                command.PersonalAccessToken!,
                cancellationToken);
            if (secretResult.IsError) return secretResult.Errors;
        }

        repo.Update(config);
        return Result.Updated;
    }

    private async Task<ErrorOr<bool>> VerifyConnectionAsync(
        UpdateInfraConfigRepositoryCommand command,
        GitProviderType providerType,
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
        if (personalAccessTokenResult.IsError) return personalAccessTokenResult.Errors;

        var verificationResult = await ProjectRepositoryConnectionVerifier.VerifyBranchesAsync(
            gitProviderFactory,
            providerType,
            command.RepositoryUrl,
            personalAccessTokenResult.Value,
            command.DefaultBranch,
            cancellationToken);
        if (verificationResult.IsError) return verificationResult.Errors;

        return !string.IsNullOrWhiteSpace(command.PersonalAccessToken);
    }

    private async Task<ErrorOr<string>> GetVerificationPersonalAccessTokenAsync(
        UpdateInfraConfigRepositoryCommand command,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.PersonalAccessToken))
            return command.PersonalAccessToken;

        return await keyVaultSecretClient.GetSecretAsync(
                ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(command.RepositoryId),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
