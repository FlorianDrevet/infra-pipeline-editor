using BicepGenerator.Domain.Common.Models;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class Location : ValueObject
{
    public enum LocationEnum
    {
        EastUS,
        WestUS,
        CentralUS,
        NorthEurope,
        WestEurope,
        SoutheastAsia,
        EastAsia,
        AustraliaEast,
        JapanEast,
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