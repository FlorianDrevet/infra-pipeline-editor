using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;

/// <summary>Adds a private endpoint configuration to an Azure resource.</summary>
public record AddPrivateEndpointCommand(
    AzureResourceId ResourceId,
    AzureResourceId SubnetId,
    string GroupId,
    bool AutoApproval = true,
    AzureResourceId? PrivateDnsZoneId = null,
    string? CustomNetworkInterfaceName = null
) : ICommand<PrivateEndpointConfigResult>;
