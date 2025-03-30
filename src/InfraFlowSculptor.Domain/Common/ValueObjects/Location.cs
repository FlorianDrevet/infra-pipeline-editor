using System.ComponentModel;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class Location : ValueObject
{
    public enum LocationEnum
    {
        [Description("europe")]
        Europe,
        [Description("france")]
        France,
    }

    public LocationEnum Value { get; protected set; }

    public Location()
    {
    }

    public Location(LocationEnum value)
    {
        this.Value = value;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}