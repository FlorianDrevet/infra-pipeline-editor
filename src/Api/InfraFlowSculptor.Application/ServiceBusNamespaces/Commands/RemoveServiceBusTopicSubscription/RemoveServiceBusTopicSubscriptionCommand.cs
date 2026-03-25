using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusTopicSubscription;

/// <summary>Command to remove a topic subscription from a Service Bus Namespace.</summary>
/// <param name="ServiceBusNamespaceId">The parent Service Bus Namespace identifier.</param>
/// <param name="SubscriptionId">The topic subscription identifier to remove.</param>
public record RemoveServiceBusTopicSubscriptionCommand(
    AzureResourceId ServiceBusNamespaceId,
    Guid SubscriptionId
) : IRequest<ErrorOr<ServiceBusNamespaceResult>>;
