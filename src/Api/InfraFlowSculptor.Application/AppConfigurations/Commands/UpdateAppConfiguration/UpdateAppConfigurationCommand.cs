using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;

/// <summary>Command to update an existing App Configuration resource.</summary>
/// <param name="Id">The App Configuration identifier.</param>
/// <param name="Name">The new display name.</param>
/// <param name="Location">The new Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record UpdateAppConfigurationCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyList<AppConfigurationEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<AppConfigurationResult>>;
