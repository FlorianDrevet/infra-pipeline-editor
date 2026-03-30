using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Azure region where a resource or resource group is deployed.</summary>
public class Location(Location.LocationEnum value) : EnumValueObject<Location.LocationEnum>(value)
{
    /// <summary>Supported Azure regions.</summary>
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