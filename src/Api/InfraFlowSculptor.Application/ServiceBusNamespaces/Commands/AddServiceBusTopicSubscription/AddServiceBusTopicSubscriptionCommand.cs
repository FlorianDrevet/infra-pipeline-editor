using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;

/// <summary>Command to add a topic subscription to a Service Bus Namespace.</summary>
/// <param name="ServiceBusNamespaceId">The parent Service Bus Namespace identifier.</param>
/// <param name="TopicName">The topic name.</param>
/// <param name="SubscriptionName">The subscription name within the topic.</param>
public record AddServiceBusTopicSubscriptionCommand(
    AzureResourceId ServiceBusNamespaceId,
    string TopicName,
    string SubscriptionName
) : IRequest<ErrorOr<ServiceBusNamespaceResult>>;
