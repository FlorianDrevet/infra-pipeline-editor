using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Common;

/// <summary>Application-layer result DTO for the Event Hub Namespace aggregate.</summary>
public record EventHubNamespaceResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyCollection<EventHubNamespaceEnvironmentConfigData> EnvironmentSettings,
    IReadOnlyCollection<EventHubResult> EventHubs,
    IReadOnlyCollection<EventHubConsumerGroupResult> ConsumerGroups
);

/// <summary>Application-layer result for an Event Hub.</summary>
public record EventHubResult(Guid Id, string Name);

/// <summary>Application-layer result for an Event Hub Consumer Group.</summary>
public record EventHubConsumerGroupResult(Guid Id, string EventHubName, string ConsumerGroupName);
