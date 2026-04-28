using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext Context;

    protected BaseRepository(TContext context)
    {
        this.Context = context;
    }

    public virtual Task<TEntity> AddAsync(TEntity entity)
    {
        var res = Context.Set<TEntity>().Add(entity);
        return Task.FromResult(res.Entity);
    }

    public virtual async Task<bool> DeleteAsync(ValueObject id)
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        Context.Set<TEntity>().Remove(entity);
        return true;
    }

    public virtual async Task<TEntity?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(cancellationToken: cancellationToken, keyValues: [id]);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(
        ValueObject id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder,
        CancellationToken cancellationToken = default)
    {
        var query = queryBuilder(Context.Set<TEntity>());

        var entityType = Context.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException(
                $"Entity type {typeof(TEntity).Name} is not part of the EF Core model. Ensure it is registered in the DbContext configuration.");

        var keyProperty = entityType.FindPrimaryKey()?.Properties.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Entity type {typeof(TEntity).Name} has no primary key configured. Verify that the entity has a primary key defined via fluent configuration.");

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(parameter, keyProperty.PropertyInfo!);
        var constant = Expression.Constant(id, property.Type);
        var body = Expression.Equal(property, constant);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.FromResult(entity);
    }
}
