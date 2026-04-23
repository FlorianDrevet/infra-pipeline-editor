using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
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

    /// <summary>Override to eagerly load the per-config Repositories collection (MultiRepo layout).</summary>
    public override async Task<InfrastructureConfig?> GetByIdAsync(
        Domain.Common.Models.ValueObject id, CancellationToken cancellationToken = default)
    {
        if (id is not InfrastructureConfigId typedId)
            return await base.GetByIdAsync(id, cancellationToken);

        return await Context.InfrastructureConfigs
            .Include(c => c.Repositories)
            .FirstOrDefaultAsync(c => c.Id == typedId, cancellationToken);
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
            .Include(c => c.ResourceAbbreviationOverrides)
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

    /// <inheritdoc />
    public async Task<List<InfraConfigSummary>> GetConfigSummariesForUserAsync(
        UserId userId, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .AsNoTracking()
            .Where(c => Context.ProjectMembers.Any(pm => pm.ProjectId == c.ProjectId && pm.UserId == userId))
            .Select(c => new InfraConfigSummary(c.Id.Value, c.Name.Value))
            .ToListAsync(cancellationToken);
    }
}
