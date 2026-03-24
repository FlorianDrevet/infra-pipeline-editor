/**
 * Azure resource types matching backend AzureResource discriminator values
 */
export enum ResourceTypeEnum {
  KeyVault = 'KeyVault',
  RedisCache = 'RedisCache',
  StorageAccount = 'StorageAccount',
  AppServicePlan = 'AppServicePlan',
  WebApp = 'WebApp',
  FunctionApp = 'FunctionApp',
  UserAssignedIdentity = 'UserAssignedIdentity',
  AppConfiguration = 'AppConfiguration',
  ContainerAppEnvironment = 'ContainerAppEnvironment',
  ContainerApp = 'ContainerApp',
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
  AppServicePlan: 'dns',
  WebApp: 'language',
  FunctionApp: 'bolt',
  UserAssignedIdentity: 'fingerprint',
  AppConfiguration: 'tune',
  ContainerAppEnvironment: 'cloud_queue',
  ContainerApp: 'view_in_ar',
};

/**
 * Standard abbreviations per resource type, matching backend ResourceAbbreviationCatalog.
 */
export const RESOURCE_TYPE_ABBREVIATIONS: Record<string, string> = {
  KeyVault: 'kv',
  RedisCache: 'redis',
  StorageAccount: 'stg',
  AppServicePlan: 'asp',
  WebApp: 'app',
  FunctionApp: 'func',
  UserAssignedIdentity: 'id',
  AppConfiguration: 'appcs',
  ResourceGroup: 'rg',
  ContainerAppEnvironment: 'cae',
  ContainerApp: 'ca',
};
