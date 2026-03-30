using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure Key Vault (Standard or Premium).</summary>
public class Sku(Sku.SkuEnum value) : EnumValueObject<Sku.SkuEnum>(value)
{
    /// <summary>Available Key Vault pricing tiers.</summary>
    public enum SkuEnum
    {
        Standard,
        Premium
    }
}