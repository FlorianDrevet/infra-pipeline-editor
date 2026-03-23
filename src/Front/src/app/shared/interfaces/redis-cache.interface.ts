// ─── Environment Settings ────────────────────────────────────────────────────

export interface RedisCacheEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  capacity?: number | null;
  redisVersion?: number | null;
  enableNonSslPort?: boolean | null;
  minimumTlsVersion?: string | null;
  maxMemoryPolicy?: string | null;
}

export interface RedisCacheEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  capacity: number | null;
  redisVersion: number | null;
  enableNonSslPort: boolean | null;
  minimumTlsVersion: string | null;
  maxMemoryPolicy: string | null;
}

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
  environmentSettings: RedisCacheEnvironmentConfigResponse[];
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
  environmentSettings?: RedisCacheEnvironmentConfigEntry[];
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
  environmentSettings?: RedisCacheEnvironmentConfigEntry[];
}
