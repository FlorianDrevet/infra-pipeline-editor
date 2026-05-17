using InfraFlowSculptor.Domain.Common.BaseModels;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class AzureResourceRepository<TEntity> : BaseRepository<TEntity, ProjectDbContext> where TEntity : AzureResource
{
    public AzureResourceRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<TEntity?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .Include(r => r.DependsOn)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<TEntity?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .Include(r => r.DependsOn)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
