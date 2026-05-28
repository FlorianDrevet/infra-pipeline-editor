import { DsSelectOption } from '../../../../shared/components/ds';
import { ResourceTypeEnum } from '../../enums/resource-type.enum';

export const NETWORKING_MODE_OPTIONS: DsSelectOption[] = [
  { value: 'Simplified', label: 'CONFIG_DETAIL_NETWORKING.MODE.SIMPLIFIED' },
  { value: 'Standard', label: 'CONFIG_DETAIL_NETWORKING.MODE.STANDARD' },
  { value: 'Advanced', label: 'CONFIG_DETAIL_NETWORKING.MODE.ADVANCED' },
];

export const VNET_SOURCE_TYPE_OPTIONS: DsSelectOption[] = [
  { value: 'CreateNew', label: 'CONFIG_DETAIL_NETWORKING.VNET.CREATE_NEW' },
  { value: 'UseExisting', label: 'CONFIG_DETAIL_NETWORKING.VNET.USE_EXISTING' },
  { value: 'UseHubSpoke', label: 'CONFIG_DETAIL_NETWORKING.VNET.USE_HUB_SPOKE' },
];

export const DNS_MODE_OPTIONS: DsSelectOption[] = [
  { value: 'AutoManaged', label: 'CONFIG_DETAIL_NETWORKING.DNS.AUTO_MANAGED' },
  { value: 'CentralizedHub', label: 'CONFIG_DETAIL_NETWORKING.DNS.CENTRALIZED_HUB' },
  { value: 'Custom', label: 'CONFIG_DETAIL_NETWORKING.DNS.CUSTOM' },
];

/**
 * Resource types that support privatization via Private Endpoint.
 * Mirrors backend PrivateEndpointGroupIdCatalog.
 */
export const PRIVATIZABLE_RESOURCE_TYPES: ReadonlySet<string> = new Set<string>([
  ResourceTypeEnum.KeyVault,
  ResourceTypeEnum.StorageAccount,
  ResourceTypeEnum.AppConfiguration,
  ResourceTypeEnum.CosmosDb,
  ResourceTypeEnum.SqlServer,
  ResourceTypeEnum.RedisCache,
  ResourceTypeEnum.ServiceBusNamespace,
  ResourceTypeEnum.EventHubNamespace,
  ResourceTypeEnum.ContainerRegistry,
  ResourceTypeEnum.WebApp,
  ResourceTypeEnum.FunctionApp,
  ResourceTypeEnum.ApplicationInsights,
  ResourceTypeEnum.LogAnalyticsWorkspace,
  ResourceTypeEnum.DocumentIntelligence,
]);
