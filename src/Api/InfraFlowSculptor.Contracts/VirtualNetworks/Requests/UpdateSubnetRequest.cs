namespace InfraFlowSculptor.Contracts.VirtualNetworks.Requests;

/// <summary>Request body to update an existing subnet within a Virtual Network.</summary>
public record UpdateSubnetRequest(
    string Name,
    string AddressPrefix,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId
);
