using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IProjectRepository"/>.
/// </summary>
public sealed class ProjectRepository(ProjectDbContext context)
    : BaseRepository<Project, ProjectDbContext>(context), IProjectRepository
{
    /// <inheritdoc />
    public async Task<Project?> GetByIdWithMembersAsync(
        ProjectId id, CancellationToken cancellationToken = default)
    {
        return await Context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<List<Project>> GetAllForUserAsync(
        UserId userId, CancellationToken cancellationToken = default)
    {
        return await Context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Project?> GetByIdWithConfigurationsAsync(
        ProjectId id, CancellationToken cancellationToken = default)
    {
        return await Context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .Include(p => p.Configurations)
                .ThenInclude(c => c.Members)
                    .ThenInclude(m => m.User!)
            .Include(p => p.Configurations)
                .ThenInclude(c => c.EnvironmentDefinitions)
            .Include(p => p.Configurations)
                .ThenInclude(c => c.ResourceNamingTemplates)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
