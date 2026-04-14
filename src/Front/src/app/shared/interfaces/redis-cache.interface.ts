// ─── Environment Settings ────────────────────────────────────────────────────

export interface RedisCacheEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  capacity?: number | null;
  maxMemoryPolicy?: string | null;
}

export interface RedisCacheEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  capacity: number | null;
  maxMemoryPolicy: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface RedisCacheResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  redisVersion: number | null;
  enableNonSslPort: boolean;
  minimumTlsVersion: string | null;
  disableAccessKeyAuthentication: boolean;
  enableAadAuth: boolean;
  environmentSettings: RedisCacheEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateRedisCacheRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  redisVersion?: number | null;
  enableNonSslPort: boolean;
  minimumTlsVersion?: string | null;
  disableAccessKeyAuthentication: boolean;
  enableAadAuth: boolean;
  environmentSettings?: RedisCacheEnvironmentConfigEntry[];
}

export interface UpdateRedisCacheRequest {
  name: string;
  location: string;
  redisVersion?: number | null;
  enableNonSslPort: boolean;
  minimumTlsVersion?: string | null;
  disableAccessKeyAuthentication: boolean;
  enableAadAuth: boolean;
  environmentSettings?: RedisCacheEnvironmentConfigEntry[];
}
