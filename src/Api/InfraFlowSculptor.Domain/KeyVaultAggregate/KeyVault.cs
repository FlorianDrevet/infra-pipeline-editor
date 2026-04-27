using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate;

/// <summary>
/// Represents an Azure Key Vault resource (<c>Microsoft.KeyVault/vaults</c>).
/// Stores secrets, keys, and certificates with configurable RBAC and protection policies.
/// </summary>
public sealed class KeyVault : AzureResource
{
    private readonly List<KeyVaultEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Key Vault.</summary>
    public IReadOnlyCollection<KeyVaultEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Whether RBAC authorization is enabled for data-plane access.</summary>
    public bool EnableRbacAuthorization { get; private set; }

    /// <summary>Whether the vault is enabled for deployment (VM certificate retrieval).</summary>
    public bool EnabledForDeployment { get; private set; }

    /// <summary>Whether the vault is enabled for disk encryption.</summary>
    public bool EnabledForDiskEncryption { get; private set; }

    /// <summary>Whether the vault is enabled for ARM template deployment.</summary>
    public bool EnabledForTemplateDeployment { get; private set; }

    /// <summary>Whether purge protection is enabled (prevents permanent deletion during retention period).</summary>
    public bool EnablePurgeProtection { get; private set; }

    /// <summary>Whether soft delete is enabled.</summary>
    public bool EnableSoftDelete { get; private set; }

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        new[]
        {
            ParameterUsage.Secret
        };

    /// <summary>Adds a secret parameter usage to this Key Vault.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the parameter is not flagged as secret.</exception>
    public void AddSecret(ParameterDefinition parameter)
    {
        if (!parameter.IsSecret)
            throw new InvalidOperationException("Only secret parameters can be stored in KeyVault");

        AddParameterUsage(parameter, ParameterUsage.Secret);
    }
    
    private KeyVault()
    {
    }
    
    /// <summary>Updates the resource-level properties of this Key Vault.</summary>
    public void Update(
        Name name,
        Location location,
        bool enableRbacAuthorization,
        bool enabledForDeployment,
        bool enabledForDiskEncryption,
        bool enabledForTemplateDeployment,
        bool enablePurgeProtection,
        bool enableSoftDelete)
    {
        Name = name;
        Location = location;

        if (IsExisting)
            return;

        EnableRbacAuthorization = enableRbacAuthorization;
        EnabledForDeployment = enabledForDeployment;
        EnabledForDiskEncryption = enabledForDiskEncryption;
        EnabledForTemplateDeployment = enabledForTemplateDeployment;
        EnablePurgeProtection = enablePurgeProtection;
        EnableSoftDelete = enableSoftDelete;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetEnvironmentSettings(string environmentName, Sku? sku)
    {
        if (IsExisting)
            return;

        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku);
        }
        else
        {
            _environmentSettings.Add(
                KeyVaultEnvironmentSettings.Create(Id, environmentName, sku));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, Sku? Sku)> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var (envName, sku) in settings)
        {
            _environmentSettings.Add(
                KeyVaultEnvironmentSettings.Create(Id, envName, sku));
        }
    }

    /// <summary>Creates a new <see cref="KeyVault"/> with a generated identifier and secure defaults.</summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="enableRbacAuthorization">Whether RBAC authorization is enabled for data-plane access.</param>
    /// <param name="enabledForDeployment">Whether the vault is enabled for deployment.</param>
    /// <param name="enabledForDiskEncryption">Whether the vault is enabled for disk encryption.</param>
    /// <param name="enabledForTemplateDeployment">Whether the vault is enabled for ARM template deployment.</param>
    /// <param name="enablePurgeProtection">Whether purge protection is enabled.</param>
    /// <param name="enableSoftDelete">Whether soft delete is enabled.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    public static KeyVault Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        bool enableRbacAuthorization = true,
        bool enabledForDeployment = false,
        bool enabledForDiskEncryption = false,
        bool enabledForTemplateDeployment = false,
        bool enablePurgeProtection = true,
        bool enableSoftDelete = true,
        IReadOnlyList<(string EnvironmentName, Sku? Sku)>? environmentSettings = null,
        bool isExisting = false)
    {
        var keyVault = new KeyVault
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            EnableRbacAuthorization = enableRbacAuthorization,
            EnabledForDeployment = enabledForDeployment,
            EnabledForDiskEncryption = enabledForDiskEncryption,
            EnabledForTemplateDeployment = enabledForTemplateDeployment,
            EnablePurgeProtection = enablePurgeProtection,
            EnableSoftDelete = enableSoftDelete
        };

        if (!isExisting && environmentSettings is not null)
            keyVault.SetAllEnvironmentSettings(environmentSettings);

        return keyVault;
    }
}
