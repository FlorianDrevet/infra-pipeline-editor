using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class InfrastructureConfigRepository : BaseRepository<InfrastructureConfig, ProjectDbContext>, IInfrastructureConfigRepository
{
    public InfrastructureConfigRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .Include(c => c.CrossConfigReferences)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<InfrastructureConfig>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // With project-level membership, list configs by joining through Projects
        return await Context.InfrastructureConfigs
            .AsNoTracking()
            .Where(c => Context.ProjectMembers.Any(pm => pm.ProjectId == c.ProjectId && pm.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<InfrastructureConfig?> GetByIdWithNamingTemplatesAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .Include(c => c.ResourceNamingTemplates)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<InfrastructureConfig>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }
}
