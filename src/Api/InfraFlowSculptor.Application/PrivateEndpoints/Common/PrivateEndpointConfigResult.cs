using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Common;

/// <summary>Application-layer result for a private endpoint configuration.</summary>
public record PrivateEndpointConfigResult(
    PrivateEndpointConfigId Id,
    AzureResourceId ResourceId,
    AzureResourceId SubnetId,
    string GroupId,
    bool AutoApproval,
    AzureResourceId? PrivateDnsZoneId,
    string? CustomNetworkInterfaceName);
