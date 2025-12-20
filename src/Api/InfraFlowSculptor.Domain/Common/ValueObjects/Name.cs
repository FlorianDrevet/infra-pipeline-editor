using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Name : SingleValueObject<string>
{
    private Name() { }

    public Name(string value) : base(value)
    {
    }
}