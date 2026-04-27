import { StorageAccountSubResourcesResponse } from './storage-account.interface';

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ResourceGroupResponse {
  id: string;
  infraConfigId: string;
  name: string;
  location: string;
}

export interface AzureResourceResponse {
  id: string;
  resourceType: string;
  name: string;
  location: string;
  parentResourceId?: string;
  configuredEnvironments?: string[];
  isExisting?: boolean;
  storageSubResources?: StorageAccountSubResourcesResponse;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateResourceGroupRequest {
  infraConfigId: string;
  name: string;
  location: string;
  isExisting?: boolean;
}
