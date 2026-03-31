using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository implementation for the <see cref="EventHubNamespace"/> aggregate.</summary>
public sealed class EventHubNamespaceRepository : AzureResourceRepository<EventHubNamespace>, IEventHubNamespaceRepository
{
    /// <summary>Initializes a new instance of the <see cref="EventHubNamespaceRepository"/> class.</summary>
    public EventHubNamespaceRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<EventHubNamespace?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<EventHubNamespace>()
            .Include(eh => eh.DependsOn)
            .Include(eh => eh.EnvironmentSettings)
            .Include(eh => eh.EventHubs)
            .Include(eh => eh.ConsumerGroups)
            .FirstOrDefaultAsync(eh => eh.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<EventHubNamespace>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<EventHubNamespace>()
            .Include(eh => eh.DependsOn)
            .Include(eh => eh.EnvironmentSettings)
            .Include(eh => eh.EventHubs)
            .Include(eh => eh.ConsumerGroups)
            .Where(eh => eh.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
