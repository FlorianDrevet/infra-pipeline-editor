using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;

/// <summary>Command to remove an app setting from a resource.</summary>
/// <param name="ResourceId">Identifier of the resource.</param>
/// <param name="AppSettingId">Identifier of the app setting to remove.</param>
public record RemoveAppSettingCommand(
    AzureResourceId ResourceId,
    AppSettingId AppSettingId
) : IRequest<ErrorOr<Deleted>>;
