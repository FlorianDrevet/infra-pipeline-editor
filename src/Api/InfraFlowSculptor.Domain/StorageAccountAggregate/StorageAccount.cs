using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
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

    private readonly List<CorsRule> _corsRules = new();
    public IReadOnlyList<CorsRule> CorsRules => _corsRules.AsReadOnly();

    private readonly List<StorageAccountEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Storage Account.</summary>
    public IReadOnlyCollection<StorageAccountEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the storage account kind (e.g. StorageV2, BlobStorage).</summary>
    public StorageAccountKind Kind { get; private set; } = null!;

    /// <summary>Gets the default access tier (Hot, Cool, Premium).</summary>
    public StorageAccessTier AccessTier { get; private set; } = null!;

    /// <summary>Gets whether public blob access is allowed.</summary>
    public bool AllowBlobPublicAccess { get; private set; }

    /// <summary>Gets whether HTTPS-only traffic is enforced.</summary>
    public bool EnableHttpsTrafficOnly { get; private set; }

    /// <summary>Gets the minimum TLS version for client connections.</summary>
    public StorageAccountTlsVersion MinimumTlsVersion { get; private set; } = null!;

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private StorageAccount()
    {
    }

    /// <summary>
    /// Adds a new blob container to this storage account.
    /// Returns a conflict error if a container with the same name (case-insensitive) already exists.
    /// </summary>
    public ErrorOr<BlobContainer> AddBlobContainer(string name, BlobContainerPublicAccess publicAccess)
    {
        if (_blobContainers.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Errors.StorageAccount.DuplicateBlobContainerName(name);

        var container = BlobContainer.Create(Id, name, publicAccess);
        _blobContainers.Add(container);
        return container;
    }

    /// <summary>
    /// Adds a new queue to this storage account.
    /// Returns a conflict error if a queue with the same name (case-insensitive) already exists.
    /// </summary>
    public ErrorOr<StorageQueue> AddQueue(string name)
    {
        if (_queues.Any(q => string.Equals(q.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Errors.StorageAccount.DuplicateQueueName(name);

        var queue = StorageQueue.Create(Id, name);
        _queues.Add(queue);
        return queue;
    }

    /// <summary>
    /// Adds a new table to this storage account.
    /// Returns a conflict error if a table with the same name (case-insensitive) already exists.
    /// </summary>
    public ErrorOr<StorageTable> AddTable(string name)
    {
        if (_tables.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Errors.StorageAccount.DuplicateTableName(name);

        var table = StorageTable.Create(Id, name);
        _tables.Add(table);
        return table;
    }

    /// <summary>
    /// Updates the public access level of an existing blob container.
    /// Returns a not-found error if no container with the given identifier exists.
    /// </summary>
    public ErrorOr<Updated> UpdateBlobContainerPublicAccess(BlobContainerId containerId, BlobContainerPublicAccess publicAccess)
    {
        var container = _blobContainers.FirstOrDefault(c => c.Id == containerId);
        if (container is null)
            return Errors.StorageAccount.BlobContainerNotFoundError(containerId);

        container.UpdatePublicAccess(publicAccess);
        return Result.Updated;
    }

    /// <summary>Updates the resource-level properties of this Storage Account.</summary>
    public void Update(
        Name name,
        Location location,
        StorageAccountKind kind,
        StorageAccessTier accessTier,
        bool allowBlobPublicAccess,
        bool enableHttpsTrafficOnly,
        StorageAccountTlsVersion minimumTlsVersion)
    {
        Name = name;
        Location = location;
        Kind = kind;
        AccessTier = accessTier;
        AllowBlobPublicAccess = allowBlobPublicAccess;
        EnableHttpsTrafficOnly = enableHttpsTrafficOnly;
        MinimumTlsVersion = minimumTlsVersion;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        StorageAccountSku? sku)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku);
        }
        else
        {
            _environmentSettings.Add(
                StorageAccountEnvironmentSettings.Create(Id, environmentName, sku));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, StorageAccountSku? Sku)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                StorageAccountEnvironmentSettings.Create(Id, s.EnvironmentName, s.Sku));
        }
    }

    public void SetCorsRules(
        IReadOnlyList<(
            IReadOnlyList<string> AllowedOrigins,
            IReadOnlyList<string> AllowedMethods,
            IReadOnlyList<string> AllowedHeaders,
            IReadOnlyList<string> ExposedHeaders,
            int MaxAgeInSeconds)> corsRules)
    {
        _corsRules.Clear();

        foreach (var rule in corsRules)
        {
            _corsRules.Add(CorsRule.Create(
                Id,
                rule.AllowedOrigins,
                rule.AllowedMethods,
                rule.AllowedHeaders,
                rule.ExposedHeaders,
                rule.MaxAgeInSeconds));
        }
    }

    /// <summary>Creates a new Storage Account with resource-level configuration.</summary>
    public static StorageAccount Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        StorageAccountKind kind,
        StorageAccessTier accessTier,
        bool allowBlobPublicAccess,
        bool enableHttpsTrafficOnly,
        StorageAccountTlsVersion minimumTlsVersion,
        IReadOnlyList<(string EnvironmentName, StorageAccountSku? Sku)>? environmentSettings = null,
        IReadOnlyList<(
            IReadOnlyList<string> AllowedOrigins,
            IReadOnlyList<string> AllowedMethods,
            IReadOnlyList<string> AllowedHeaders,
            IReadOnlyList<string> ExposedHeaders,
            int MaxAgeInSeconds)>? corsRules = null)
    {
        var storageAccount = new StorageAccount
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Kind = kind,
            AccessTier = accessTier,
            AllowBlobPublicAccess = allowBlobPublicAccess,
            EnableHttpsTrafficOnly = enableHttpsTrafficOnly,
            MinimumTlsVersion = minimumTlsVersion
        };

        if (environmentSettings is not null)
            storageAccount.SetAllEnvironmentSettings(environmentSettings);

        if (corsRules is not null)
            storageAccount.SetCorsRules(corsRules);

        return storageAccount;
    }
}
