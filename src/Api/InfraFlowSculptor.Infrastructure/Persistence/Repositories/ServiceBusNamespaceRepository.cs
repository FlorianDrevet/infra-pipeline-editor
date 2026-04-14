using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="ServiceBusNamespace"/> aggregate.
/// </summary>
public sealed class ServiceBusNamespaceRepository : AzureResourceRepository<ServiceBusNamespace>, IServiceBusNamespaceRepository
{
    /// <summary>Initializes a new instance of the <see cref="ServiceBusNamespaceRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public ServiceBusNamespaceRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<ServiceBusNamespace?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<ServiceBusNamespace>()
            .Include(sb => sb.DependsOn)
            .Include(sb => sb.EnvironmentSettings)
            .Include(sb => sb.Queues)
            .Include(sb => sb.TopicSubscriptions)
            .FirstOrDefaultAsync(sb => sb.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ServiceBusNamespace>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<ServiceBusNamespace>()
            .Include(sb => sb.DependsOn)
            .Include(sb => sb.EnvironmentSettings)
            .Include(sb => sb.Queues)
            .Include(sb => sb.TopicSubscriptions)
            .Where(sb => sb.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
