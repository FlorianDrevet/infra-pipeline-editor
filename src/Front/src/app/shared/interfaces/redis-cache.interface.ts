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
}
