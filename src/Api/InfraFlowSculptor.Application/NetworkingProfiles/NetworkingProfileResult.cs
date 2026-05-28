namespace InfraFlowSculptor.Application.NetworkingProfiles;

/// <summary>Result DTO for networking profile operations.</summary>
public record NetworkingProfileResult(
    string Id,
    string InfraConfigId,
    string Mode,
    string VnetSourceType,
    string? ExistingVnetResourceId,
    string? CreateNewAddressSpace,
    string? CreateNewSubnetAddressPrefix,
    string PrivateEndpointsSubnetName,
    string DnsMode,
    string? DnsHubResourceGroupId,
    string? DnsHubSubscriptionId);
