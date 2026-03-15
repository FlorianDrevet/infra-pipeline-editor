using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class AzureResourceBaseRepository(ProjectDbContext context) : IAzureResourceRepository
{
    public async Task<AzureResource?> GetByIdWithRoleAssignmentsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<AzureResource>()
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource> UpdateAsync(AzureResource resource, CancellationToken cancellationToken = default)
    {
        context.Entry(resource).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        return resource;
    }
}
