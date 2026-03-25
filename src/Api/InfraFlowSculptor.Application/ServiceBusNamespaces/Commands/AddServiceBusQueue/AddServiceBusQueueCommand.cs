using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusQueue;

/// <summary>Command to add a queue to a Service Bus Namespace.</summary>
/// <param name="ServiceBusNamespaceId">The parent Service Bus Namespace identifier.</param>
/// <param name="Name">The queue name.</param>
public record AddServiceBusQueueCommand(
    AzureResourceId ServiceBusNamespaceId,
    string Name
) : IRequest<ErrorOr<ServiceBusNamespaceResult>>;
