using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;

/// <summary>Command to permanently delete an App Configuration resource.</summary>
/// <param name="Id">The App Configuration identifier.</param>
public record DeleteAppConfigurationCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
