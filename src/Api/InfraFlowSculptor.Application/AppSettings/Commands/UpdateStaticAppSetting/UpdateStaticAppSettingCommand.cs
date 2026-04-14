using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppSettings.Commands.UpdateStaticAppSetting;

/// <summary>Command to update a static app setting's name and per-environment values.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="AppSettingId">Identifier of the app setting to update.</param>
/// <param name="Name">The new environment variable name.</param>
/// <param name="EnvironmentValues">The new per-environment values.</param>
public record UpdateStaticAppSettingCommand(
    AzureResourceId ResourceId,
    AppSettingId AppSettingId,
    string Name,
    IReadOnlyDictionary<string, string> EnvironmentValues
) : ICommand<AppSettingResult>;
