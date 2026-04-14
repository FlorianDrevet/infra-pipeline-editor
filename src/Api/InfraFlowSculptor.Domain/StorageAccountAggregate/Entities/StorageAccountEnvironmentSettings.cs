using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="StorageAccount"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class StorageAccountEnvironmentSettings : Entity<StorageAccountEnvironmentSettingsId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the performance/replication tier override.</summary>
    public StorageAccountSku? Sku { get; private set; }

    private StorageAccountEnvironmentSettings() { }

    internal StorageAccountEnvironmentSettings(
        AzureResourceId storageAccountId,
        string environmentName,
        StorageAccountSku? sku)
        : base(StorageAccountEnvironmentSettingsId.CreateUnique())
    {
        StorageAccountId = storageAccountId;
        EnvironmentName = environmentName;
        Sku = sku;
    }

    /// <summary>
    /// Creates a new <see cref="StorageAccountEnvironmentSettings"/> for the specified Storage Account and environment.
    /// </summary>
    public static StorageAccountEnvironmentSettings Create(
        AzureResourceId storageAccountId,
        string environmentName,
        StorageAccountSku? sku)
        => new(storageAccountId, environmentName, sku);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(StorageAccountSku? sku)
    {
        Sku = sku;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku.Value.ToString();
        return dict;
    }
}
