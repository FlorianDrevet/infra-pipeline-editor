using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;

/// <summary>
/// Removes the private endpoint configuration from an Azure resource.
/// </summary>
public record RemovePrivateEndpointConfigCommand(
    InfrastructureConfigId InfraConfigId,
    AzureResourceId ResourceId
) : ICommand<Success>;
