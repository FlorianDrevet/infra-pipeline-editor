using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository for <see cref="FrontDoor"/> aggregates.</summary>
public class FrontDoorRepository : AzureResourceRepository<FrontDoor>, IFrontDoorRepository
{
    public FrontDoorRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<FrontDoor?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<FrontDoor>())
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public override async Task<FrontDoor?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<FrontDoor>().AsNoTracking())
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<List<FrontDoor>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<FrontDoor>())
            .Where(f => f.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<FrontDoor> WithSubResources(IQueryable<FrontDoor> query)
    {
        return query
            .Include(f => f.DependsOn)
            .Include(f => f.Origins)
            .Include(f => f.EnvironmentSettings);
    }
}
