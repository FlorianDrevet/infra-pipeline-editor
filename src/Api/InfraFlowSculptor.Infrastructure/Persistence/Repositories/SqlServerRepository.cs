using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ISqlServerRepository"/>.</summary>
public class SqlServerRepository(ProjectDbContext context)
    : AzureResourceRepository<SqlServer>(context), ISqlServerRepository
{
    /// <inheritdoc />
    public override async Task<SqlServer?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<SqlServer>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SqlServer>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<SqlServer>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
