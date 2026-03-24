using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;

/// <summary>Command to permanently delete an App Configuration resource.</summary>
/// <param name="Id">The App Configuration identifier.</param>
public record DeleteAppConfigurationCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
