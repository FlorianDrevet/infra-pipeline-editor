// ─── Environment Settings ────────────────────────────────────────────────────

export interface StorageAccountEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
}

export interface StorageAccountEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  isExisting?: boolean;
}

export interface CorsRuleEntry {
  allowedOrigins: string[];
  allowedMethods: string[];
  allowedHeaders: string[];
  exposedHeaders: string[];
  maxAgeInSeconds: number;
}

export interface CorsRuleResponse {
  allowedOrigins: string[];
  allowedMethods: string[];
  allowedHeaders: string[];
  exposedHeaders: string[];
  maxAgeInSeconds: number;
  isExisting?: boolean;
}

export interface BlobLifecycleRuleEntry {
  ruleName: string;
  containerNames: string[];
  timeToLiveInDays: number;
}

export interface BlobLifecycleRuleResponse {
  ruleName: string;
  containerNames: string[];
  timeToLiveInDays: number;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface BlobContainerResponse {
  id: string;
  name: string;
  publicAccess: string;
  isExisting?: boolean;
}

export interface StorageQueueResponse {
  id: string;
  name: string;
  isExisting?: boolean;
}

export interface StorageTableResponse {
  id: string;
  name: string;
  isExisting?: boolean;
}

export interface StorageAccountSubResourcesResponse {
  blobContainers: BlobContainerResponse[];
  queues: StorageQueueResponse[];
  tables: StorageTableResponse[];
}

export interface StorageAccountResponse extends StorageAccountSubResourcesResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  corsRules: CorsRuleResponse[];
  tableCorsRules: CorsRuleResponse[];
  lifecycleRules: BlobLifecycleRuleResponse[];
  environmentSettings: StorageAccountEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateStorageAccountRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  corsRules?: CorsRuleEntry[];
  tableCorsRules?: CorsRuleEntry[];
  lifecycleRules?: BlobLifecycleRuleEntry[];
  environmentSettings?: StorageAccountEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateStorageAccountRequest {
  name: string;
  location: string;
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  corsRules?: CorsRuleEntry[];
  tableCorsRules?: CorsRuleEntry[];
  lifecycleRules?: BlobLifecycleRuleEntry[];
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

export interface UpdateBlobContainerPublicAccessRequest {
  publicAccess: string;
}
