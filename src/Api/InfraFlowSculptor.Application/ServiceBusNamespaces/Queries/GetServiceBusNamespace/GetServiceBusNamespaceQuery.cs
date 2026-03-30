using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Queries;

/// <summary>Query to retrieve a single Service Bus Namespace resource by identifier.</summary>
/// <param name="Id">The Service Bus Namespace identifier.</param>
public record GetServiceBusNamespaceQuery(
    AzureResourceId Id
) : IQuery<ServiceBusNamespaceResult>;
