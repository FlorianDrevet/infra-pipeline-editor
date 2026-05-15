using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository for <see cref="PrivateDnsZone"/> aggregates.</summary>
public class PrivateDnsZoneRepository : AzureResourceRepository<PrivateDnsZone>, IPrivateDnsZoneRepository
{
    public PrivateDnsZoneRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<PrivateDnsZone?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<PrivateDnsZone>())
            .FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public override async Task<PrivateDnsZone?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<PrivateDnsZone>().AsNoTracking())
            .FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public async Task<List<PrivateDnsZone>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<PrivateDnsZone>())
            .Where(z => z.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<PrivateDnsZone> WithSubResources(IQueryable<PrivateDnsZone> query)
    {
        return query
            .Include(z => z.DependsOn)
            .Include(z => z.VirtualNetworkLinks);
    }
}
