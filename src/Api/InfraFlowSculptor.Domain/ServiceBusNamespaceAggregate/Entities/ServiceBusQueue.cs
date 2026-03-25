using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

/// <summary>
/// Represents a queue within an Azure Service Bus Namespace.
/// </summary>
public sealed class ServiceBusQueue : Entity<ServiceBusQueueId>
{
    /// <summary>Gets the parent Service Bus Namespace identifier.</summary>
    public AzureResourceId ServiceBusNamespaceId { get; private set; } = null!;

    /// <summary>Gets the queue name.</summary>
    public string Name { get; private set; } = string.Empty;

    private ServiceBusQueue() { }

    /// <summary>
    /// Creates a new <see cref="ServiceBusQueue"/> for the specified namespace.
    /// </summary>
    /// <param name="serviceBusNamespaceId">The parent namespace identifier.</param>
    /// <param name="name">The queue name.</param>
    /// <returns>A new <see cref="ServiceBusQueue"/> entity.</returns>
    public static ServiceBusQueue Create(AzureResourceId serviceBusNamespaceId, string name)
        => new()
        {
            Id = ServiceBusQueueId.CreateUnique(),
            ServiceBusNamespaceId = serviceBusNamespaceId,
            Name = name
        };
}
