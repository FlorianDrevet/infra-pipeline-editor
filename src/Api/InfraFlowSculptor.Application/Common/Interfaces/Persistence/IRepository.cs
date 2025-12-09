using System.Linq.Expressions;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(ValueObject id);
}