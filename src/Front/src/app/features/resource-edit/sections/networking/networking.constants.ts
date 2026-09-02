import { ResourceTypeEnum } from '../../../../shared/resource-metadata/resource-type.metadata';

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
