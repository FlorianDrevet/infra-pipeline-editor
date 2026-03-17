using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class StorageAccountRepository : AzureResourceRepository<StorageAccount>, IStorageAccountRepository
{
    public StorageAccountRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<StorageAccount?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<StorageAccount>()
            .Include(s => s.DependsOn)
            .Include(s => s.BlobContainers)
            .Include(s => s.Queues)
            .Include(s => s.Tables)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<StorageAccount?> GetByIdWithSubResourcesAsync(AzureResourceId id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<StorageAccount>()
            .Include(s => s.DependsOn)
            .Include(s => s.BlobContainers)
            .Include(s => s.Queues)
            .Include(s => s.Tables)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<StorageAccount>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<StorageAccount>()
            .Include(s => s.DependsOn)
            .Include(s => s.BlobContainers)
            .Include(s => s.Queues)
            .Include(s => s.Tables)
            .Where(s => s.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<BlobContainer> AddBlobContainerAsync(BlobContainer container)
    {
        Context.Set<BlobContainer>().Add(container);
        await Context.SaveChangesAsync();
        return container;
    }

    public async Task<bool> RemoveBlobContainerAsync(AzureResourceId storageAccountId, BlobContainerId id)
    {
        var container = await Context.Set<BlobContainer>()
            .FirstOrDefaultAsync(c => c.Id == id && c.StorageAccountId == storageAccountId);
        if (container is null)
            return false;

        Context.Set<BlobContainer>().Remove(container);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<StorageQueue> AddQueueAsync(StorageQueue queue)
    {
        Context.Set<StorageQueue>().Add(queue);
        await Context.SaveChangesAsync();
        return queue;
    }

    public async Task<bool> RemoveQueueAsync(AzureResourceId storageAccountId, StorageQueueId id)
    {
        var queue = await Context.Set<StorageQueue>()
            .FirstOrDefaultAsync(q => q.Id == id && q.StorageAccountId == storageAccountId);
        if (queue is null)
            return false;

        Context.Set<StorageQueue>().Remove(queue);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<StorageTable> AddTableAsync(StorageTable table)
    {
        Context.Set<StorageTable>().Add(table);
        await Context.SaveChangesAsync();
        return table;
    }

    public async Task<bool> RemoveTableAsync(AzureResourceId storageAccountId, StorageTableId id)
    {
        var table = await Context.Set<StorageTable>()
            .FirstOrDefaultAsync(t => t.Id == id && t.StorageAccountId == storageAccountId);
        if (table is null)
            return false;

        Context.Set<StorageTable>().Remove(table);
        await Context.SaveChangesAsync();
        return true;
    }
}
