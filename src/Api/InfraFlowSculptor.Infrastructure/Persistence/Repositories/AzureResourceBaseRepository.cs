using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class AzureResourceBaseRepository(ProjectDbContext context) : IAzureResourceRepository
{
    public async Task<AzureResource?> GetByIdAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdReadOnlyAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdWithRoleAssignmentsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdWithRoleAssignmentsReadOnlyAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .AsNoTracking()
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdWithAppSettingsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await WithAppSettings(context.AzureResources)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdWithRoleAssignmentsAndAppSettingsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await WithAppSettings(context.AzureResources)
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AzureResource?> GetByIdWithRoleAssignmentsAndAppSettingsReadOnlyAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await WithAppSettings(context.AzureResources.AsNoTracking())
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AzureResource?> GetByIdWithSecureParameterMappingsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .Include(r => r.SecureParameterMappings)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AzureResource?> GetByIdWithCustomDomainsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .Include(r => r.CustomDomains)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AzureResource?> GetByIdWithPrivateEndpointsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await WithPrivateEndpoints(context.AzureResources)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AzureResource?> GetByIdWithPrivateEndpointsReadOnlyAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await WithPrivateEndpoints(context.AzureResources)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    private static IQueryable<AzureResource> WithAppSettings(IQueryable<AzureResource> query)
    {
        return query
            .Include(r => r.AppSettings)
                .ThenInclude(s => s.EnvironmentValues);
    }

    private static IQueryable<AzureResource> WithPrivateEndpoints(IQueryable<AzureResource> query)
    {
        return query
            .Include(r => r.ResourceGroup)
            .Include(r => r.PrivateEndpointConfigs);
    }

    public async Task<bool> ExistsAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        return await context.AzureResources
            .AnyAsync(r => r.Id == id, cancellationToken);
    }

    public Task<AzureResource> UpdateAsync(AzureResource resource, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(resource);
    }

    /// <inheritdoc />
    public async Task<List<RoleAssignment>> GetRoleAssignmentsByIdentityIdAsync(
        AzureResourceId identityId,
        CancellationToken cancellationToken = default)
    {
        return await context.RoleAssignments
            .Where(r => r.UserAssignedIdentityId == identityId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> RevertRoleAssignmentsToSystemAssignedAsync(
        AzureResourceId identityId,
        CancellationToken cancellationToken = default)
    {
        return await context.RoleAssignments
            .Where(r => r.UserAssignedIdentityId == identityId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.ManagedIdentityType,
                    new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.SystemAssigned))
                .SetProperty(r => r.UserAssignedIdentityId, (AzureResourceId?)null),
                cancellationToken);
    }
}
