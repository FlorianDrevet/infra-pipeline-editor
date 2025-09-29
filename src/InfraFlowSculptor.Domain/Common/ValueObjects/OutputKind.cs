using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class OutputKind : ValueObject
{
    public enum OutputKindEnum
    {
        ConnectingString        
    }

    public OutputKindEnum Value { get; protected set; }

    public OutputKind()
    {
    }

    public OutputKind(OutputKindEnum value)
    {
        this.Value = value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}