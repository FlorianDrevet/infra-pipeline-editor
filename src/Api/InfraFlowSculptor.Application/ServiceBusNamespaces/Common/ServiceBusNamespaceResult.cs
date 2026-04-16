using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Common;

/// <summary>
/// Application-layer result DTO for the Service Bus Namespace aggregate.
/// </summary>
public record ServiceBusNamespaceResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyCollection<ServiceBusNamespaceEnvironmentConfigData> EnvironmentSettings,
    IReadOnlyCollection<ServiceBusQueueResult> Queues,
    IReadOnlyCollection<ServiceBusTopicSubscriptionResult> TopicSubscriptions
);

/// <summary>Application-layer result for a Service Bus Queue.</summary>
public record ServiceBusQueueResult(Guid Id, string Name);

/// <summary>Application-layer result for a Service Bus Topic Subscription.</summary>
public record ServiceBusTopicSubscriptionResult(Guid Id, string TopicName, string SubscriptionName);
