using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;

/// <summary>Handles the <see cref="SetProjectLayoutPresetCommand"/>.</summary>
public sealed class SetProjectLayoutPresetCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<SetProjectLayoutPresetCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetProjectLayoutPresetCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var layoutPresetResult = EnumValueObjectParser.Parse<LayoutPresetEnum, LayoutPreset>(
            command.Preset,
            static parsed => new LayoutPreset(parsed),
            Errors.Project.InvalidLayoutPreset);
        if (layoutPresetResult.IsError)
            return layoutPresetResult.Errors;

        var setResult = project.SetLayoutPreset(layoutPresetResult.Value);
        if (setResult.IsError)
            return setResult.Errors;

        projectRepository.Update(project);

        return Result.Success;
    }
}
