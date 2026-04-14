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
  LogAnalyticsWorkspace = 'LogAnalyticsWorkspace',
  ApplicationInsights = 'ApplicationInsights',
  CosmosDb = 'CosmosDb',
  SqlServer = 'SqlServer',
  SqlDatabase = 'SqlDatabase',
  ServiceBusNamespace = 'ServiceBusNamespace',
  EventHubNamespace = 'EventHubNamespace',
  ContainerRegistry = 'ContainerRegistry',
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
  LogAnalyticsWorkspace: 'analytics',
  ApplicationInsights: 'monitoring',
  CosmosDb: 'public',
  SqlServer: 'dns',
  SqlDatabase: 'table_chart',
  ServiceBusNamespace: 'swap_horiz',
  EventHubNamespace: 'swap_vert',
  ContainerRegistry: 'inventory_2',
};

export interface ResourceTypeCategory {
  labelKey: string;
  icon: string;
  types: ResourceTypeEnum[];
}

/**
 * Resource types grouped by functional category for the type-picker UI.
 */
export const RESOURCE_TYPE_CATEGORIES: ResourceTypeCategory[] = [
  {
    labelKey: 'CONFIG_DETAIL.RESOURCES.CATEGORY_COMPUTE',
    icon: 'computer',
    types: [
      ResourceTypeEnum.AppServicePlan,
      ResourceTypeEnum.WebApp,
      ResourceTypeEnum.FunctionApp,
      ResourceTypeEnum.ContainerAppEnvironment,
      ResourceTypeEnum.ContainerApp,
    ],
  },
  {
    labelKey: 'CONFIG_DETAIL.RESOURCES.CATEGORY_STORAGE_DB',
    icon: 'storage',
    types: [
      ResourceTypeEnum.StorageAccount,
      ResourceTypeEnum.CosmosDb,
      ResourceTypeEnum.RedisCache,
      ResourceTypeEnum.SqlServer,
      ResourceTypeEnum.SqlDatabase,
      ResourceTypeEnum.ContainerRegistry,
    ],
  },
  {
    labelKey: 'CONFIG_DETAIL.RESOURCES.CATEGORY_SECURITY',
    icon: 'shield',
    types: [
      ResourceTypeEnum.KeyVault,
      ResourceTypeEnum.UserAssignedIdentity,
    ],
  },
  {
    labelKey: 'CONFIG_DETAIL.RESOURCES.CATEGORY_MESSAGING',
    icon: 'swap_horiz',
    types: [
      ResourceTypeEnum.ServiceBusNamespace,
      ResourceTypeEnum.EventHubNamespace,
    ],
  },
  {
    labelKey: 'CONFIG_DETAIL.RESOURCES.CATEGORY_MONITORING',
    icon: 'monitoring',
    types: [
      ResourceTypeEnum.LogAnalyticsWorkspace,
      ResourceTypeEnum.ApplicationInsights,
      ResourceTypeEnum.AppConfiguration,
    ],
  },
];

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
  LogAnalyticsWorkspace: 'law',
  ApplicationInsights: 'appi',
  CosmosDb: 'cosmos',
  SqlServer: 'sql',
  SqlDatabase: 'sqldb',
  ServiceBusNamespace: 'sb',
  EventHubNamespace: 'evhns',
  ContainerRegistry: 'acr',
};

/**
 * Defines parent resource types and the child types that belong to them.
 * Used to visually group child resources under their parent in the resource list.
 */
export const PARENT_CHILD_RESOURCE_TYPES: Record<string, string[]> = {
  [ResourceTypeEnum.AppServicePlan]: [ResourceTypeEnum.WebApp, ResourceTypeEnum.FunctionApp],
  [ResourceTypeEnum.ContainerAppEnvironment]: [ResourceTypeEnum.ContainerApp],
  [ResourceTypeEnum.LogAnalyticsWorkspace]: [ResourceTypeEnum.ApplicationInsights],
  [ResourceTypeEnum.SqlServer]: [ResourceTypeEnum.SqlDatabase],
  [ResourceTypeEnum.StorageAccount]: [],
};

/**
 * Set of all resource types that act as children and should not appear at the top level.
 */
export const CHILD_RESOURCE_TYPES = new Set<string>(
  Object.values(PARENT_CHILD_RESOURCE_TYPES).flat(),
);
