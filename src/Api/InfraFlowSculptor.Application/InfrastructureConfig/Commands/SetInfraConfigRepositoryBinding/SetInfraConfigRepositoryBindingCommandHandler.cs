using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigRepositoryBinding;

/// <summary>Handles the <see cref="SetInfraConfigRepositoryBindingCommand"/>.</summary>
public sealed class SetInfraConfigRepositoryBindingCommandHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    IProjectAccessService accessService)
    : ICommandHandler<SetInfraConfigRepositoryBindingCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetInfraConfigRepositoryBindingCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await configRepository.GetByIdAsync(command.ConfigId);
        if (infraConfig is null || infraConfig.ProjectId != command.ProjectId)
            return Errors.InfrastructureConfig.NotFoundError(command.ConfigId);

        // Clear binding when alias is null/empty.
        if (string.IsNullOrWhiteSpace(command.RepositoryAlias))
        {
            var clearResult = infraConfig.SetRepositoryBinding(null);
            if (clearResult.IsError)
                return clearResult.Errors;

            await configRepository.UpdateAsync(infraConfig);
            return Result.Success;
        }

        var aliasResult = RepositoryAlias.Create(command.RepositoryAlias);
        if (aliasResult.IsError)
            return aliasResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.GetRepositoryByAlias(aliasResult.Value) is null)
            return Errors.ProjectRepository.NotFound(aliasResult.Value);

        var bindingResult = RepositoryBinding.Create(
            aliasResult.Value,
            command.Branch,
            command.InfraPath,
            command.PipelinePath);
        if (bindingResult.IsError)
            return bindingResult.Errors;

        var setResult = infraConfig.SetRepositoryBinding(bindingResult.Value);
        if (setResult.IsError)
            return setResult.Errors;

        await configRepository.UpdateAsync(infraConfig);
        return Result.Success;
    }
}
