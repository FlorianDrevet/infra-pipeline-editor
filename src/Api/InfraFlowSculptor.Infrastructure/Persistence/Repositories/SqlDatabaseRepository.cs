using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ISqlDatabaseRepository"/>.</summary>
public class SqlDatabaseRepository(ProjectDbContext context)
    : AzureResourceRepository<SqlDatabase>(context), ISqlDatabaseRepository
{
    /// <inheritdoc />
    public override async Task<SqlDatabase?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<SqlDatabase>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SqlDatabase>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<SqlDatabase>()
            .Include(x => x.DependsOn)
            .Include(x => x.EnvironmentSettings)
            .Where(x => x.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
