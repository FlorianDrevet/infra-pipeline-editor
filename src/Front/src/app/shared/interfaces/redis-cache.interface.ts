import { ResourceEnvironmentConfigEntry, ResourceEnvironmentConfigResponse } from './resource-environment-config.interface';

// ─── Responses ───────────────────────────────────────────────────────────────

export interface RedisCacheResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
  capacity: number;
  redisVersion: number;
  enableNonSslPort: boolean;
  minimumTlsVersion: string;
  maxMemoryPolicy: string;
  environmentConfigs: ResourceEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateRedisCacheRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
  redisVersion: number;
  enableNonSslPort: boolean;
  minimumTlsVersion: string;
  maxMemoryPolicy: string;
  capacity: number;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
}

export interface UpdateRedisCacheRequest {
  name: string;
  location: string;
  sku: string;
  redisVersion: number;
  enableNonSslPort: boolean;
  minimumTlsVersion: string;
  maxMemoryPolicy: string;
  capacity: number;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
}
