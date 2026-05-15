using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core repository for <see cref="NetworkSecurityGroup"/> aggregates.</summary>
public class NetworkSecurityGroupRepository : AzureResourceRepository<NetworkSecurityGroup>, INetworkSecurityGroupRepository
{
    public NetworkSecurityGroupRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<NetworkSecurityGroup?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<NetworkSecurityGroup>())
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public override async Task<NetworkSecurityGroup?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<NetworkSecurityGroup>().AsNoTracking())
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<List<NetworkSecurityGroup>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<NetworkSecurityGroup>())
            .Where(n => n.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<NetworkSecurityGroup> WithSubResources(IQueryable<NetworkSecurityGroup> query)
    {
        return query
            .Include(n => n.DependsOn)
            .Include(n => n.SecurityRules);
    }
}
