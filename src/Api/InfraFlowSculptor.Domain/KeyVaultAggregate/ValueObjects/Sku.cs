using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

public class Sku : ValueObject
{
    public enum SkuEnum
    {
        Standard,
        Premium
    }

    public SkuEnum Value { get; protected set; }

    public Sku()
    {
    }

    public Sku(SkuEnum value)
    {
        this.Value = value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}