using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;

/// <summary>Command to remove a queue from a Service Bus Namespace.</summary>
/// <param name="ServiceBusNamespaceId">The parent Service Bus Namespace identifier.</param>
/// <param name="QueueId">The queue identifier to remove.</param>
public record RemoveServiceBusQueueCommand(
    AzureResourceId ServiceBusNamespaceId,
    Guid QueueId
) : ICommand<ServiceBusNamespaceResult>;
