using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Azure region where a resource or resource group is deployed.</summary>
public sealed class Location(Location.LocationEnum value) : EnumValueObject<Location.LocationEnum>(value)
{
    /// <summary>Gets the canonical Azure region key used when no explicit location is provided.</summary>
    public static string DefaultAzureRegionKey => ToAzureRegionKey(LocationEnum.WestEurope);

    /// <summary>Converts a domain <see cref="LocationEnum"/> value to the Azure wire-format region key.</summary>
    /// <param name="location">The strongly typed location to convert.</param>
    /// <returns>The lowercase Azure region key expected by contracts, generators, and ARM payloads.</returns>
    public static string ToAzureRegionKey(LocationEnum location)
    {
        return location switch
        {
            LocationEnum.FranceCentral => "francecentral",
            LocationEnum.FranceSouth => "francesouth",
            LocationEnum.UKSouth => "uksouth",
            LocationEnum.WestEurope => "westeurope",
            LocationEnum.GermanyWestCentral => "germanywestcentral",
            LocationEnum.SwitzerlandNorth => "switzerlandnorth",
            LocationEnum.ItalyNorth => "italynorth",
            LocationEnum.NorthEurope => "northeurope",
            LocationEnum.SpainCentral => "spaincentral",
            LocationEnum.NorwayEast => "norwayeast",
            LocationEnum.PolandCentral => "polandcentral",
            LocationEnum.SwedenCentral => "swedencentral",
            LocationEnum.QatarCentral => "qatarcentral",
            LocationEnum.UAENorth => "uaenorth",
            LocationEnum.CanadaEast => "canadaeast",
            LocationEnum.CanadaCentral => "canadacentral",
            LocationEnum.EastUS => "eastus",
            LocationEnum.EastUS2 => "eastus2",
            LocationEnum.CentralIndia => "centralindia",
            LocationEnum.CentralUS => "centralus",
            LocationEnum.SouthCentralUS => "southcentralus",
            LocationEnum.WestUS2 => "westus2",
            LocationEnum.SouthAfricaNorth => "southafricanorth",
            LocationEnum.WestUS3 => "westus3",
            LocationEnum.WestUS => "westus",
            LocationEnum.KoreaCentral => "koreacentral",
            LocationEnum.BrazilSouth => "brazilsouth",
            LocationEnum.EastAsia => "eastasia",
            LocationEnum.JapanEast => "japaneast",
            LocationEnum.SoutheastAsia => "southeastasia",
            LocationEnum.AustraliaEast => "australiaeast",
            _ => DefaultAzureRegionKey,
        };
    }

    /// <summary>Converts a <see cref="Location"/> value object to the Azure wire-format region key.</summary>
    /// <param name="location">The location value object to convert.</param>
    /// <returns>The lowercase Azure region key expected by contracts, generators, and ARM payloads.</returns>
    public static string ToAzureRegionKey(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return ToAzureRegionKey(location.Value);
    }

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