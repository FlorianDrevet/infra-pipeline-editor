using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class InfrastructureConfigRepository : BaseRepository<InfrastructureConfig, ProjectDbContext>, IInfrastructureConfigRepository
{
    public InfrastructureConfigRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id.Value == id.Value, cancellationToken);
    }

    public async Task<List<InfrastructureConfig>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await Context.InfrastructureConfigs
            .AsNoTracking()
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }
}