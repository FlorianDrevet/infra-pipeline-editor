using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class Location(Location.LocationEnum value) : EnumValueObject<Location.LocationEnum>(value)
{
    public enum LocationEnum
    {
        FranceCentral,
        FranceSouth,
        UKSouth,
        WestEurope,
        GermanyWestCentral,
        SwitzerlandNorth,
        ItalyNorth,
        NorthEurope,
        SpainCentral,
        NorwayEast,
        PolandCentral,
        SwedenCentral,
        QatarCentral,
        UAENorth,
        CanadaEast,
        CanadaCentral,
        EastUS,
        EastUS2,
        CentralIndia,
        CentralUS,
        SouthCentralUS,
        WestUS2,
        SouthAfricaNorth,
        WestUS3,
        WestUS,
        KoreaCentral,
        BrazilSouth,
        EastAsia,
        JapanEast,
        SoutheastAsia,
        AustraliaEast,
    }
}