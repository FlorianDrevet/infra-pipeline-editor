using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitConfig;

/// <summary>Handles the <see cref="SetProjectGitConfigCommand"/>.</summary>
public sealed class SetProjectGitConfigCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultClient)
    : IRequestHandler<SetProjectGitConfigCommand, ErrorOr<Success>>
{
    /// <summary>The Key Vault secret name prefix for Git PATs.</summary>
    private const string SecretNamePrefix = "git-pat-";

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetProjectGitConfigCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (!Enum.TryParse<GitProviderTypeEnum>(command.ProviderType, ignoreCase: true, out var providerTypeEnum))
            return Errors.GitRepository.InvalidRepositoryUrl();

        var providerType = new GitProviderType(providerTypeEnum);

        // Store the PAT in the centralized Key Vault
        var secretName = $"{SecretNamePrefix}{project.Id.Value}";
        var storeResult = await keyVaultClient.SetSecretAsync(secretName, command.PersonalAccessToken, cancellationToken);
        if (storeResult.IsError)
            return storeResult.Errors;

        project.SetGitRepositoryConfiguration(
            providerType,
            command.RepositoryUrl,
            command.DefaultBranch,
            command.BasePath);

        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }
}
