using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Commands.UpdatePrivateEndpoint;

/// <summary>Updates an existing private endpoint configuration.</summary>
public record UpdatePrivateEndpointCommand(
    AzureResourceId ResourceId,
    PrivateEndpointConfigId ConfigId,
    AzureResourceId SubnetId,
    string GroupId,
    bool AutoApproval = true,
    AzureResourceId? PrivateDnsZoneId = null,
    string? CustomNetworkInterfaceName = null
) : ICommand<PrivateEndpointConfigResult>;
