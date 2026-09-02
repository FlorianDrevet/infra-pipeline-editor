using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetPrivateEndpointConfig;

/// <summary>
/// Configures a private endpoint on an Azure resource with a selected VNet, subnet, and DNS mode.
/// </summary>
public record SetPrivateEndpointConfigCommand(
    InfrastructureConfigId InfraConfigId,
    AzureResourceId ResourceId,
    AzureResourceId VirtualNetworkId,
    string SubnetName,
    string DnsMode,
    string? DnsHubResourceGroupId,
    string? DnsHubSubscriptionId
) : ICommand<Success>;
