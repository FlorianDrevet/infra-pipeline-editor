using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Infrastructure.Persistence;

/// <summary>
/// Unit of Work backed by the shared <see cref="ProjectDbContext"/>.
/// All repositories share the same scoped DbContext, so a single
/// <see cref="SaveChangesAsync"/> call commits every tracked mutation atomically.
/// </summary>
public sealed class UnitOfWork(ProjectDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
