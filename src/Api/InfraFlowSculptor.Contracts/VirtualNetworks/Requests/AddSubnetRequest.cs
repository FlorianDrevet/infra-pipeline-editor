namespace InfraFlowSculptor.Contracts.VirtualNetworks.Requests;

/// <summary>Request body to add a subnet to a Virtual Network.</summary>
public record AddSubnetRequest(
    string Name,
    string AddressPrefix,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId
);
