// ─── Environment Settings ────────────────────────────────────────────────────

export interface StorageAccountEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  kind?: string | null;
  accessTier?: string | null;
  allowBlobPublicAccess?: boolean | null;
  enableHttpsTrafficOnly?: boolean | null;
  minimumTlsVersion?: string | null;
}

export interface StorageAccountEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  kind: string | null;
  accessTier: string | null;
  allowBlobPublicAccess: boolean | null;
  enableHttpsTrafficOnly: boolean | null;
  minimumTlsVersion: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface BlobContainerResponse {
  id: string;
  name: string;
  publicAccess: string;
}

export interface StorageQueueResponse {
  id: string;
  name: string;
}

export interface StorageTableResponse {
  id: string;
  name: string;
}

export interface StorageAccountResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  blobContainers: BlobContainerResponse[];
  queues: StorageQueueResponse[];
  tables: StorageTableResponse[];
  environmentSettings: StorageAccountEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateStorageAccountRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: StorageAccountEnvironmentConfigEntry[];
}

export interface UpdateStorageAccountRequest {
  name: string;
  location: string;
  environmentSettings?: StorageAccountEnvironmentConfigEntry[];
}

export interface AddBlobContainerRequest {
  name: string;
  publicAccess: string;
}

export interface AddQueueRequest {
  name: string;
}

export interface AddTableRequest {
  name: string;
}
