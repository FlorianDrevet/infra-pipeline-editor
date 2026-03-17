using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate;

public class StorageAccount : AzureResource
{
    public required StorageAccountSku Sku { get; set; }

    public required StorageAccountKind Kind { get; set; }

    /// <summary>
    /// The access tier for the storage account (Hot or Cool). Applies to BlobStorage and StorageV2 kinds.
    /// </summary>
    public required StorageAccessTier AccessTier { get; set; }

    /// <summary>
    /// Whether to allow public read access to blobs and containers.
    /// </summary>
    public required bool AllowBlobPublicAccess { get; set; }

    /// <summary>
    /// Whether to enforce HTTPS-only traffic to the storage account.
    /// </summary>
    public required bool EnableHttpsTrafficOnly { get; set; }

    public required StorageAccountTlsVersion MinimumTlsVersion { get; set; }

    private readonly List<BlobContainer> _blobContainers = new();
    public IReadOnlyList<BlobContainer> BlobContainers => _blobContainers.AsReadOnly();

    private readonly List<StorageQueue> _queues = new();
    public IReadOnlyList<StorageQueue> Queues => _queues.AsReadOnly();

    private readonly List<StorageTable> _tables = new();
    public IReadOnlyList<StorageTable> Tables => _tables.AsReadOnly();

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private StorageAccount()
    {
    }

    public BlobContainer AddBlobContainer(string name, BlobContainerPublicAccess publicAccess)
    {
        var container = BlobContainer.Create(Id, name, publicAccess);
        _blobContainers.Add(container);
        return container;
    }

    public StorageQueue AddQueue(string name)
    {
        var queue = StorageQueue.Create(Id, name);
        _queues.Add(queue);
        return queue;
    }

    public StorageTable AddTable(string name)
    {
        var table = StorageTable.Create(Id, name);
        _tables.Add(table);
        return table;
    }

    public void Update(
        Name name,
        Location location,
        StorageAccountSettings settings)
    {
        Name = name;
        Location = location;
        Sku = settings.Sku;
        Kind = settings.Kind;
        AccessTier = settings.AccessTier;
        AllowBlobPublicAccess = settings.AllowBlobPublicAccess;
        EnableHttpsTrafficOnly = settings.EnableHttpsTrafficOnly;
        MinimumTlsVersion = settings.MinimumTlsVersion;
    }

    public static StorageAccount Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        StorageAccountSettings settings)
    {
        return new StorageAccount
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Sku = settings.Sku,
            Kind = settings.Kind,
            AccessTier = settings.AccessTier,
            AllowBlobPublicAccess = settings.AllowBlobPublicAccess,
            EnableHttpsTrafficOnly = settings.EnableHttpsTrafficOnly,
            MinimumTlsVersion = settings.MinimumTlsVersion
        };
    }
}
