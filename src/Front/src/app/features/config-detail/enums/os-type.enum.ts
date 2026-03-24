export enum OsTypeEnum {
  Windows = 'Windows',
  Linux = 'Linux',
}

export const OS_TYPE_OPTIONS = Object.entries(OsTypeEnum).map(([key, value]) => ({
  label: key,
  value,
}));
