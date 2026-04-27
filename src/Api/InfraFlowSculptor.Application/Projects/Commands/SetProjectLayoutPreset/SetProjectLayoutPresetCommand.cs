using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;

/// <summary>
/// Command to update the layout preset of a project. The preset is purely informative in V1
/// and does not drive generation; it is consumed by the frontend wizard to remember the user's
/// initial topology choice.
/// </summary>
/// <param name="ProjectId">Identifier of the target project.</param>
/// <param name="Preset">Preset name. Valid values: AllInOne, SplitInfraCode, MultiRepo, Custom.</param>
public record SetProjectLayoutPresetCommand(
    ProjectId ProjectId,
    string Preset
) : ICommand<Success>;
