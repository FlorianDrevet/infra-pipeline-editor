using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for the <see cref="CosmosDb"/> aggregate.
/// </summary>
public sealed class CosmosDbRepository : AzureResourceRepository<CosmosDb>, ICosmosDbRepository
{
    /// <summary>Initializes a new instance of the <see cref="CosmosDbRepository"/> class.</summary>
    /// <param name="context">The database context.</param>
    public CosmosDbRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<CosmosDb?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<CosmosDb>()
            .Include(c => c.DependsOn)
            .Include(c => c.EnvironmentSettings)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<CosmosDb>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<CosmosDb>()
            .Include(c => c.DependsOn)
            .Include(c => c.EnvironmentSettings)
            .Where(c => c.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
