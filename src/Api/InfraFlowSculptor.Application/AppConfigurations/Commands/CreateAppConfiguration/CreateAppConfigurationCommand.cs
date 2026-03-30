using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;

/// <summary>Command to create a new App Configuration resource inside a Resource Group.</summary>
/// <param name="ResourceGroupId">The parent resource group identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="Location">The Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record CreateAppConfigurationCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<AppConfigurationEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<AppConfigurationResult>;
