// ─── Environment Settings ────────────────────────────────────────────────────

export interface StorageAccountEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
}

export interface StorageAccountEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
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
  kind: string;
  accessTier: string;
  allowBlobPublicAccess: boolean;
  enableHttpsTrafficOnly: boolean;
  minimumTlsVersion: string;
  corsRules: CorsRuleResponse[];
  tableCorsRules: CorsRuleResponse[];
  lifecycleRules: BlobLifecycleRuleResponse[];
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
