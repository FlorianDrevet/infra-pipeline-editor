using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;

/// <summary>Removes a private endpoint configuration from an Azure resource.</summary>
public record RemovePrivateEndpointCommand(
    AzureResourceId ResourceId,
    PrivateEndpointConfigId ConfigId
) : ICommand<Deleted>;
