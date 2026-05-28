using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class DocumentIntelligenceRepository : AzureResourceRepository<DocumentIntelligence>, IDocumentIntelligenceRepository
{
    public DocumentIntelligenceRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<DocumentIntelligence?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<DocumentIntelligence>())
            .FirstOrDefaultAsync(di => di.Id == id, cancellationToken);
    }

    public override async Task<DocumentIntelligence?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<DocumentIntelligence>().AsNoTracking())
            .FirstOrDefaultAsync(di => di.Id == id, cancellationToken);
    }

    public async Task<List<DocumentIntelligence>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<DocumentIntelligence>())
            .Where(di => di.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<DocumentIntelligence> WithSubResources(IQueryable<DocumentIntelligence> query)
    {
        return query
            .Include(di => di.DependsOn)
            .Include(di => di.EnvironmentSettings);
    }
}
