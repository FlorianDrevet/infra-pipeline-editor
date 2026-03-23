using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="KeyVault"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class KeyVaultEnvironmentSettings : Entity<KeyVaultEnvironmentSettingsId>
{
    /// <summary>Gets the parent Key Vault identifier.</summary>
    public AzureResourceId KeyVaultId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override for this environment.</summary>
    public Sku? Sku { get; private set; }

    private KeyVaultEnvironmentSettings() { }

    internal KeyVaultEnvironmentSettings(
        AzureResourceId keyVaultId,
        string environmentName,
        Sku? sku)
        : base(KeyVaultEnvironmentSettingsId.CreateUnique())
    {
        KeyVaultId = keyVaultId;
        EnvironmentName = environmentName;
        Sku = sku;
    }

    /// <summary>
    /// Creates a new <see cref="KeyVaultEnvironmentSettings"/> for the specified Key Vault and environment.
    /// </summary>
    public static KeyVaultEnvironmentSettings Create(
        AzureResourceId keyVaultId,
        string environmentName,
        Sku? sku)
        => new(keyVaultId, environmentName, sku);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(Sku? sku)
    {
        Sku = sku;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku.Value.ToString().ToLower();
        return dict;
    }
}
