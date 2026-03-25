using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository interface for the <see cref="ServiceBusNamespace"/> aggregate.</summary>
public interface IServiceBusNamespaceRepository : IRepository<ServiceBusNamespace>
{
    /// <summary>Retrieves all Service Bus Namespaces belonging to the specified resource group.</summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of matching Service Bus Namespace aggregates.</returns>
    Task<List<ServiceBusNamespace>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
