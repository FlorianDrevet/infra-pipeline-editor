using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PrivateEndpoints.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateEndpoints.Queries.GetPrivateEndpointConfigs;

/// <summary>Gets all private endpoint configurations for a resource.</summary>
public record GetPrivateEndpointConfigsQuery(AzureResourceId ResourceId)
    : IQuery<IReadOnlyList<PrivateEndpointConfigResult>>;
