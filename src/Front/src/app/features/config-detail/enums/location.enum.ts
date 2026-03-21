/**
 * Azure location enum matching backend Location.LocationEnum
 */
export enum LocationEnum {
  EastUS = 'EastUS',
  WestUS = 'WestUS',
  CentralUS = 'CentralUS',
  NorthEurope = 'NorthEurope',
  WestEurope = 'WestEurope',
  SoutheastAsia = 'SoutheastAsia',
  EastAsia = 'EastAsia',
  AustraliaEast = 'AustraliaEast',
  JapanEast = 'JapanEast',
}

/**
 * Dropdown options for LocationEnum
 */
export const LOCATION_OPTIONS = Object.entries(LocationEnum).map(([key, value]) => ({
  label: key,
  value,
}));
