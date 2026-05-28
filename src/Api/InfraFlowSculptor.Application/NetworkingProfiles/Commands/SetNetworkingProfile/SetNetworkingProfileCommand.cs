using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetNetworkingProfile;

/// <summary>Creates or updates the networking profile for an infrastructure configuration.</summary>
public record SetNetworkingProfileCommand(
    InfrastructureConfigId InfraConfigId,
    NetworkingMode.Mode Mode,
    VnetSource.SourceType VnetSourceType,
    string? ExistingVnetResourceId,
    string? CreateNewAddressSpace,
    string? CreateNewSubnetAddressPrefix,
    string? PrivateEndpointsSubnetName,
    DnsMode.Mode DnsMode,
    string? DnsHubResourceGroupId,
    string? DnsHubSubscriptionId
) : ICommand<NetworkingProfileResult>;
