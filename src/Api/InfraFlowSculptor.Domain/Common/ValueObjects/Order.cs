using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Order : SingleValueObject<int>
{
    private Order() { }

    public Order(int value) : base(value)
    {
    }
}
