namespace InfraFlowSculptor.Contracts.PrivateEndpoints.Responses;

/// <summary>Response representing a private endpoint configuration.</summary>
public record PrivateEndpointConfigResponse(
    string Id,
    string ResourceId,
    string SubnetId,
    string GroupId,
    bool AutoApproval,
    string? PrivateDnsZoneId,
    string? CustomNetworkInterfaceName);
