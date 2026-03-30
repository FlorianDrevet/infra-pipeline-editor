using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Zero-based display order for environment definitions.</summary>
public sealed class Order : SingleValueObject<int>
{
    private Order() { }

    public Order(int value) : base(value)
    {
    }
}
