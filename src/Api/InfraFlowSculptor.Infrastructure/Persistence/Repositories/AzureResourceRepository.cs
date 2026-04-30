using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class AzureResourceRepository<TEntity>: BaseRepository<TEntity, ProjectDbContext> where TEntity : AzureResource
{
    public AzureResourceRepository(ProjectDbContext context) : base(context)
    {
    }
    
    public override async Task<TEntity?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<TEntity>()
            .Include(r => r.DependsOn)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<TEntity?> GetByIdAsync(
        ValueObject id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var query = queryBuilder(Context.Set<TEntity>());
        return await query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}