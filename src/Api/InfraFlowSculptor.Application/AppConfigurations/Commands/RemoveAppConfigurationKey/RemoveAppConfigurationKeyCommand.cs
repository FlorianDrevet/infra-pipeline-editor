using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;

/// <summary>Command to remove a configuration key from an App Configuration resource.</summary>
/// <param name="AppConfigurationId">Identifier of the App Configuration resource.</param>
/// <param name="AppConfigurationKeyId">Identifier of the configuration key to remove.</param>
public record RemoveAppConfigurationKeyCommand(
    AzureResourceId AppConfigurationId,
    AppConfigurationKeyId AppConfigurationKeyId
) : ICommand<Deleted>;
