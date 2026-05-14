using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Queries;

/// <summary>Retrieves a single Virtual Network by identifier.</summary>
public record GetVirtualNetworkQuery(
    AzureResourceId Id
) : IQuery<VirtualNetworkResult>;
