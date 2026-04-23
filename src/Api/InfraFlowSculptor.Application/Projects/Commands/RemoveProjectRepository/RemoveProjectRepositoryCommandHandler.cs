using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;

/// <summary>Handles the <see cref="RemoveProjectRepositoryCommand"/>.</summary>
public sealed class RemoveProjectRepositoryCommandHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IProjectAccessService accessService)
    : ICommandHandler<RemoveProjectRepositoryCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectRepositoryCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var existing = project.Repositories.FirstOrDefault(r => r.Id == command.RepositoryId);
        if (existing is null)
            return Errors.ProjectRepository.NotFound(command.RepositoryId);

        var alias = existing.Alias;

        // Refuse the deletion if at least one infrastructure configuration is still bound
        // to this repository alias to avoid orphan bindings.
        var inUse = await infraConfigRepository.AnyBoundToRepositoryAliasAsync(
            command.ProjectId, alias, cancellationToken);
        if (inUse)
            return Errors.ProjectRepository.RepositoryInUse(alias.Value);

        var removeResult = project.RemoveRepository(command.RepositoryId);
        if (removeResult.IsError)
            return removeResult.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
