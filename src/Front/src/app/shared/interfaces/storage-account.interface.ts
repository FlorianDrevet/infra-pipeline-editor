import { ResourceEnvironmentConfigEntry, ResourceEnvironmentConfigResponse } from './resource-environment-config.interface';

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
  sku: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  blobContainers: BlobContainerResponse[];
  queues: StorageQueueResponse[];
  tables: StorageTableResponse[];
  environmentConfigs: ResourceEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateStorageAccountRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
}

export interface UpdateStorageAccountRequest {
  name: string;
  location: string;
  sku: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
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
