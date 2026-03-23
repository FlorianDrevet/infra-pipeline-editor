using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate;

public class KeyVault: AzureResource
{
    public required Sku Sku { get; set; }

    private readonly List<KeyVaultEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Key Vault.</summary>
    public IReadOnlyCollection<KeyVaultEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();
    
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        new[]
        {
            ParameterUsage.Secret
        };

    public void AddSecret(ParameterDefinition parameter)
    {
        if (!parameter.IsSecret)
            throw new InvalidOperationException("Only secret parameters can be stored in KeyVault");

        AddParameterUsage(parameter, ParameterUsage.Secret);
    }
    
    private KeyVault()
    {
    }
    
    public void Update(Name name, Location location, Sku sku)
    {
        Name = name;
        Location = location;
        Sku = sku;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(string environmentName, Sku? sku)
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
                KeyVaultEnvironmentSettings.Create(Id, environmentName, sku));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, Sku? Sku)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku) in settings)
        {
            _environmentSettings.Add(
                KeyVaultEnvironmentSettings.Create(Id, envName, sku));
        }
    }

    public static KeyVault Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        Sku sku,
        IReadOnlyList<(string EnvironmentName, Sku? Sku)>? environmentSettings = null)
    {
        var keyVault = new KeyVault
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Sku = sku
        };

        if (environmentSettings is not null)
            keyVault.SetAllEnvironmentSettings(environmentSettings);

        return keyVault;
    }
}