import { ResourceEnvironmentConfigEntry, ResourceEnvironmentConfigResponse } from './resource-environment-config.interface';

// ─── Responses ───────────────────────────────────────────────────────────────

export interface KeyVaultResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
  environmentConfigs: ResourceEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateKeyVaultRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
}

export interface UpdateKeyVaultRequest {
  name: string;
  location: string;
  sku: string;
  environmentConfigs?: ResourceEnvironmentConfigEntry[];
}
