using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.KeyVaults.Requests;

/// <summary>Common properties shared by create and update Key Vault requests.</summary>
public abstract class KeyVaultRequestBase
{
    /// <summary>Display name for the Key Vault resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Key Vault will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Whether RBAC authorization is enabled for data-plane access.</summary>
    public bool EnableRbacAuthorization { get; init; } = true;

    /// <summary>Whether the vault is enabled for deployment (VM certificate retrieval).</summary>
    public bool EnabledForDeployment { get; init; }

    /// <summary>Whether the vault is enabled for disk encryption.</summary>
    public bool EnabledForDiskEncryption { get; init; }

    /// <summary>Whether the vault is enabled for ARM template deployment.</summary>
    public bool EnabledForTemplateDeployment { get; init; }

    /// <summary>Whether purge protection is enabled.</summary>
    public bool EnablePurgeProtection { get; init; } = true;

    /// <summary>Whether soft delete is enabled.</summary>
    public bool EnableSoftDelete { get; init; } = true;

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<KeyVaultEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Key Vault.</summary>
public class KeyVaultEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override.</summary>
    [EnumValidation(typeof(Sku.SkuEnum))]
    public string? Sku { get; init; }
}

/// <summary>Response DTO for a typed per-environment Key Vault configuration.</summary>
public record KeyVaultEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku);
