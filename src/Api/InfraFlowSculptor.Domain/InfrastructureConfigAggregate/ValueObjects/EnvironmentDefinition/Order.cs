using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class Order: SingleValueObject<int>
{
    private Order() { }

    public Order(int value) : base(value)
    {
    }
}