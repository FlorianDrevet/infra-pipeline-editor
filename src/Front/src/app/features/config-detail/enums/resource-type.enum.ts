/**
 * Azure resource types matching backend AzureResource discriminator values
 */
export enum ResourceTypeEnum {
  KeyVault = 'KeyVault',
  RedisCache = 'RedisCache',
  StorageAccount = 'StorageAccount',
}

/**
 * Dropdown options for ResourceTypeEnum
 */
export const RESOURCE_TYPE_OPTIONS = Object.entries(ResourceTypeEnum).map(([key, value]) => ({
  label: key,
  value,
}));

/**
 * Material icons per resource type
 */
export const RESOURCE_TYPE_ICONS: Record<string, string> = {
  KeyVault: 'vpn_key',
  RedisCache: 'memory',
  StorageAccount: 'storage',
};

/**
 * Standard abbreviations per resource type, matching backend ResourceAbbreviationCatalog.
 */
export const RESOURCE_TYPE_ABBREVIATIONS: Record<string, string> = {
  KeyVault: 'kv',
  RedisCache: 'redis',
  StorageAccount: 'stg',
  ResourceGroup: 'rg',
};
