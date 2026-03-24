using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Command to add an app setting (environment variable) to a compute resource.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="Name">The environment variable name.</param>
/// <param name="StaticValue">The static value (null when using a resource output reference).</param>
/// <param name="SourceResourceId">Identifier of the source resource for output reference (null for static values).</param>
/// <param name="SourceOutputName">The output name on the source resource (null for static values).</param>
public record AddAppSettingCommand(
    AzureResourceId ResourceId,
    string Name,
    string? StaticValue,
    AzureResourceId? SourceResourceId,
    string? SourceOutputName
) : IRequest<ErrorOr<AppSettingResult>>;
