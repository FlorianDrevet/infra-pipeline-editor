using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Common;

/// <summary>Application-layer result for a Virtual Network.</summary>
public record VirtualNetworkResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<SubnetData> Subnets,
    IReadOnlyList<VirtualNetworkEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false
);
