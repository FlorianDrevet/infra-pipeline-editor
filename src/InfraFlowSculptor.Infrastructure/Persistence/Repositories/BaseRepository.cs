using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext Context;

    public BaseRepository(TContext context)
    {
        this.Context = context;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var res = Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<bool> DeleteAsync(ValueObject id)
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<TEntity?> GetByIdAsync(ValueObject id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        await Context.SaveChangesAsync();
        return entity;
    }
}