using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate;

public class StorageAccount : AzureResource
{
    private readonly List<BlobContainer> _blobContainers = new();
    public IReadOnlyList<BlobContainer> BlobContainers => _blobContainers.AsReadOnly();

    private readonly List<StorageQueue> _queues = new();
    public IReadOnlyList<StorageQueue> Queues => _queues.AsReadOnly();

    private readonly List<StorageTable> _tables = new();
    public IReadOnlyList<StorageTable> Tables => _tables.AsReadOnly();

    private readonly List<StorageAccountEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Storage Account.</summary>
    public IReadOnlyCollection<StorageAccountEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

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
        Location location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        StorageAccountSku? sku,
        StorageAccountKind? kind,
        StorageAccessTier? accessTier,
        bool? allowBlobPublicAccess,
        bool? enableHttpsTrafficOnly,
        StorageAccountTlsVersion? minimumTlsVersion)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, kind, accessTier, allowBlobPublicAccess, enableHttpsTrafficOnly, minimumTlsVersion);
        }
        else
        {
            _environmentSettings.Add(
                StorageAccountEnvironmentSettings.Create(Id, environmentName, sku, kind, accessTier, allowBlobPublicAccess, enableHttpsTrafficOnly, minimumTlsVersion));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, StorageAccountSku? Sku, StorageAccountKind? Kind, StorageAccessTier? AccessTier, bool? AllowBlobPublicAccess, bool? EnableHttpsTrafficOnly, StorageAccountTlsVersion? MinimumTlsVersion)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                StorageAccountEnvironmentSettings.Create(Id, s.EnvironmentName, s.Sku, s.Kind, s.AccessTier, s.AllowBlobPublicAccess, s.EnableHttpsTrafficOnly, s.MinimumTlsVersion));
        }
    }

    public static StorageAccount Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, StorageAccountSku? Sku, StorageAccountKind? Kind, StorageAccessTier? AccessTier, bool? AllowBlobPublicAccess, bool? EnableHttpsTrafficOnly, StorageAccountTlsVersion? MinimumTlsVersion)>? environmentSettings = null)
    {
        var storageAccount = new StorageAccount
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            storageAccount.SetAllEnvironmentSettings(environmentSettings);

        return storageAccount;
    }
}
