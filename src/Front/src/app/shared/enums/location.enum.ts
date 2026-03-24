/**
 * Azure location enum matching backend Location.LocationEnum.
 * Sorted by proximity to France (closest first).
 */
export enum LocationEnum {
  FranceCentral = 'FranceCentral',
  FranceSouth = 'FranceSouth',
  UKSouth = 'UKSouth',
  WestEurope = 'WestEurope',
  GermanyWestCentral = 'GermanyWestCentral',
  SwitzerlandNorth = 'SwitzerlandNorth',
  ItalyNorth = 'ItalyNorth',
  NorthEurope = 'NorthEurope',
  SpainCentral = 'SpainCentral',
  NorwayEast = 'NorwayEast',
  PolandCentral = 'PolandCentral',
  SwedenCentral = 'SwedenCentral',
  QatarCentral = 'QatarCentral',
  UAENorth = 'UAENorth',
  CanadaEast = 'CanadaEast',
  CanadaCentral = 'CanadaCentral',
  EastUS = 'EastUS',
  EastUS2 = 'EastUS2',
  CentralIndia = 'CentralIndia',
  CentralUS = 'CentralUS',
  SouthCentralUS = 'SouthCentralUS',
  WestUS2 = 'WestUS2',
  SouthAfricaNorth = 'SouthAfricaNorth',
  WestUS3 = 'WestUS3',
  WestUS = 'WestUS',
  KoreaCentral = 'KoreaCentral',
  BrazilSouth = 'BrazilSouth',
  EastAsia = 'EastAsia',
  JapanEast = 'JapanEast',
  SoutheastAsia = 'SoutheastAsia',
  AustraliaEast = 'AustraliaEast',
}

/** Human-readable labels for Azure locations */
const LOCATION_LABELS: Record<LocationEnum, string> = {
  [LocationEnum.FranceCentral]: 'France Central (Paris)',
  [LocationEnum.FranceSouth]: 'France South (Marseille)',
  [LocationEnum.UKSouth]: 'UK South (London)',
  [LocationEnum.WestEurope]: 'West Europe (Netherlands)',
  [LocationEnum.GermanyWestCentral]: 'Germany West Central (Frankfurt)',
  [LocationEnum.SwitzerlandNorth]: 'Switzerland North (Zurich)',
  [LocationEnum.ItalyNorth]: 'Italy North (Milan)',
  [LocationEnum.NorthEurope]: 'North Europe (Ireland)',
  [LocationEnum.SpainCentral]: 'Spain Central (Madrid)',
  [LocationEnum.NorwayEast]: 'Norway East (Oslo)',
  [LocationEnum.PolandCentral]: 'Poland Central (Warsaw)',
  [LocationEnum.SwedenCentral]: 'Sweden Central (Gävle)',
  [LocationEnum.QatarCentral]: 'Qatar Central (Doha)',
  [LocationEnum.UAENorth]: 'UAE North (Dubai)',
  [LocationEnum.CanadaEast]: 'Canada East (Quebec)',
  [LocationEnum.CanadaCentral]: 'Canada Central (Toronto)',
  [LocationEnum.EastUS]: 'East US (Virginia)',
  [LocationEnum.EastUS2]: 'East US 2 (Virginia)',
  [LocationEnum.CentralIndia]: 'Central India (Pune)',
  [LocationEnum.CentralUS]: 'Central US (Iowa)',
  [LocationEnum.SouthCentralUS]: 'South Central US (Texas)',
  [LocationEnum.WestUS2]: 'West US 2 (Washington)',
  [LocationEnum.SouthAfricaNorth]: 'South Africa North (Johannesburg)',
  [LocationEnum.WestUS3]: 'West US 3 (Arizona)',
  [LocationEnum.WestUS]: 'West US (California)',
  [LocationEnum.KoreaCentral]: 'Korea Central (Seoul)',
  [LocationEnum.BrazilSouth]: 'Brazil South (São Paulo)',
  [LocationEnum.EastAsia]: 'East Asia (Hong Kong)',
  [LocationEnum.JapanEast]: 'Japan East (Tokyo)',
  [LocationEnum.SoutheastAsia]: 'Southeast Asia (Singapore)',
  [LocationEnum.AustraliaEast]: 'Australia East (Sydney)',
};

/**
 * Dropdown options for LocationEnum, sorted by proximity to France (closest first).
 * Labels include the city name for clarity.
 */
export const LOCATION_OPTIONS: { label: string; value: LocationEnum }[] = Object.values(LocationEnum).map(value => ({
  label: LOCATION_LABELS[value],
  value,
}));
