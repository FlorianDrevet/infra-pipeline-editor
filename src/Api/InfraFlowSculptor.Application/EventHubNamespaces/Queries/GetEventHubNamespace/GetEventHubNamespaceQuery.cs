using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Queries;

/// <summary>Query to retrieve a single Event Hub Namespace resource by identifier.</summary>
public record GetEventHubNamespaceQuery(
    AzureResourceId Id
) : IQuery<EventHubNamespaceResult>;
