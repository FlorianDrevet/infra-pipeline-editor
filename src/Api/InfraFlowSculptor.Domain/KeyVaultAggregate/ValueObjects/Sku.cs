using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

public class Sku(Sku.SkuEnum value) : EnumValueObject<Sku.SkuEnum>(value)
{
    public enum SkuEnum
    {
        Standard,
        Premium
    }
}