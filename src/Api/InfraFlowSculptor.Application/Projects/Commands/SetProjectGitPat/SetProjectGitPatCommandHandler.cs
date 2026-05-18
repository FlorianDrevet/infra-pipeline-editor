using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitPat;

/// <summary>
/// Handles the <see cref="SetProjectGitPatCommand"/>.
/// </summary>
public sealed class SetProjectGitPatCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IKeyVaultSecretClient keyVaultSecretClient)
    : ICommandHandler<SetProjectGitPatCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetProjectGitPatCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var repository = project.Repositories.FirstOrDefault(r => r.Id == command.RepositoryId);
        if (repository is null)
            return Errors.ProjectRepository.NotFound(command.RepositoryId);

        var secretResult = await keyVaultSecretClient.SetSecretAsync(
            ProjectGitSecretNames.GetRepositoryPatSecretName(command.RepositoryId),
            command.PersonalAccessToken.Trim(),
            cancellationToken);
        if (secretResult.IsError)
            return secretResult.Errors;

        return Result.Success;
    }
}