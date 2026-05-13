export enum OsTypeEnum {
  Windows = 'Windows',
  Linux = 'Linux',
}

export const OS_TYPE_OPTIONS = Object.entries(OsTypeEnum).map(([label, value]) => ({
  label,
  value,
}));