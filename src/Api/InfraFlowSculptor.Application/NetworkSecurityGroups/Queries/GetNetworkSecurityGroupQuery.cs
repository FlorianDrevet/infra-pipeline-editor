using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Queries;

/// <summary>Retrieves a single Network Security Group by identifier.</summary>
public record GetNetworkSecurityGroupQuery(
    AzureResourceId Id
) : IQuery<NetworkSecurityGroupResult>;
