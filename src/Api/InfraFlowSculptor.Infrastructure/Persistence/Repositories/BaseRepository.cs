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

        public virtual async Task<TEntity?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
        {
            var entityType = Context.Model.FindEntityType(typeof(TEntity))
                ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not part of the current DbContext model.");
            var primaryKey = entityType.FindPrimaryKey()
                ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not define a primary key.");

            if (primaryKey.Properties.Count != 1)
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} uses a composite key and cannot be loaded with GetByIdReadOnlyAsync.");

            var keyProperty = primaryKey.Properties[0];
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var propertyAccess = keyProperty.PropertyInfo is not null
                ? Expression.Property(parameter, keyProperty.PropertyInfo)
                : Expression.Property(parameter, keyProperty.Name);

            if (!propertyAccess.Type.IsInstanceOfType(id))
                throw new InvalidOperationException($"Identifier type {id.GetType().Name} does not match the primary key type {propertyAccess.Type.Name} for entity {typeof(TEntity).Name}.");

            var equals = Expression.Equal(propertyAccess, Expression.Constant(id, propertyAccess.Type));
            var predicate = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

            return await Context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken);
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
