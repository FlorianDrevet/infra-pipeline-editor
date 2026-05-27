using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.VerifyProjectRepositoryConnection;

/// <summary>Handles the <see cref="VerifyProjectRepositoryConnectionCommand"/>.</summary>
public sealed class VerifyProjectRepositoryConnectionCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient,
    IGitProviderFactory gitProviderFactory)
    : ICommandHandler<VerifyProjectRepositoryConnectionCommand, ProjectRepositoryConnectionVerificationResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectRepositoryConnectionVerificationResult>> Handle(
        VerifyProjectRepositoryConnectionCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (command.RepositoryId is not null
            && project.Repositories.All(repository => repository.Id != command.RepositoryId))
        {
            return Errors.ProjectRepository.NotFound(command.RepositoryId);
        }

        var providerTypeResult = EnumValueObjectParser.Parse<GitProviderTypeEnum, GitProviderType>(
            command.ProviderType,
            static parsed => new GitProviderType(parsed),
            Errors.GitRepository.InvalidProviderType);
        if (providerTypeResult.IsError)
            return providerTypeResult.Errors;

        var personalAccessToken = command.PersonalAccessToken;
        if (string.IsNullOrWhiteSpace(personalAccessToken))
        {
            if (command.RepositoryId is null)
                return Errors.ProjectRepository.PersonalAccessTokenRequired();

            var secretResult = await keyVaultSecretClient.GetSecretAsync(
                ProjectGitSecretNames.GetRepositoryPatSecretName(command.RepositoryId),
                cancellationToken);
            if (secretResult.IsError)
                return secretResult.Errors;

            personalAccessToken = secretResult.Value;
        }

        var branchesResult = await ProjectRepositoryConnectionVerifier.VerifyBranchesAsync(
            gitProviderFactory,
            providerTypeResult.Value,
            command.RepositoryUrl,
            personalAccessToken,
            requiredDefaultBranch: null,
            cancellationToken);
        if (branchesResult.IsError)
            return branchesResult.Errors;

        return new ProjectRepositoryConnectionVerificationResult(
            branchesResult.Value.Owner,
            branchesResult.Value.RepositoryName,
            branchesResult.Value.Branches,
            GetDefaultBranchCandidate(branchesResult.Value.Branches));
    }

    private static string? GetDefaultBranchCandidate(IReadOnlyList<GitBranchResult> branches)
    {
        return branches.FirstOrDefault(branch => string.Equals(branch.Name, "main", StringComparison.OrdinalIgnoreCase))?.Name
            ?? branches.FirstOrDefault()?.Name;
    }
}