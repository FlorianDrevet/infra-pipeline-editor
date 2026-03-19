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

    /// <summary>Pricing tier for the Key Vault. Accepted values: <c>Standard</c>, <c>Premium</c>.</summary>
    [Required, EnumValidation(typeof(Sku.SkuEnum))]
    public required string Sku { get; init; }
}
