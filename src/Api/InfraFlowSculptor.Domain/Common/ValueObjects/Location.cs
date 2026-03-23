using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class Location(Location.LocationEnum value) : EnumValueObject<Location.LocationEnum>(value)
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
}