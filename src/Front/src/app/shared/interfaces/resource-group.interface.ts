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
  properties?: Record<string, string>;
  storageSubResources?: StorageAccountSubResourcesResponse;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateResourceGroupRequest {
  infraConfigId: string;
  name: string;
  location: string;
  isExisting?: boolean;
}

export interface UpdateResourceGroupRequest {
  name: string;
  location: string;
}
