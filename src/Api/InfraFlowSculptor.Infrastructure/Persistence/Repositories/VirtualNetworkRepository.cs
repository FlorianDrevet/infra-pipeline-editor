using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository for <see cref="VirtualNetwork"/> aggregates.</summary>
public class VirtualNetworkRepository : AzureResourceRepository<VirtualNetwork>, IVirtualNetworkRepository
{
    public VirtualNetworkRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<VirtualNetwork?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<VirtualNetwork>())
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public override async Task<VirtualNetwork?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<VirtualNetwork>().AsNoTracking())
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<VirtualNetwork>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<VirtualNetwork>())
            .Where(v => v.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<VirtualNetwork> WithSubResources(IQueryable<VirtualNetwork> query)
    {
        return query
            .Include(v => v.DependsOn)
            .Include(v => v.Subnets)
            .Include(v => v.EnvironmentSettings);
    }
}
