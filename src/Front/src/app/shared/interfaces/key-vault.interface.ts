// ─── Environment Settings ────────────────────────────────────────────────────

export interface KeyVaultEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
}

export interface KeyVaultEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface KeyVaultResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  enableRbacAuthorization: boolean;
  enabledForDeployment: boolean;
  enabledForDiskEncryption: boolean;
  enabledForTemplateDeployment: boolean;
  enablePurgeProtection: boolean;
  enableSoftDelete: boolean;
  environmentSettings: KeyVaultEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateKeyVaultRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  enableRbacAuthorization?: boolean;
  enabledForDeployment?: boolean;
  enabledForDiskEncryption?: boolean;
  enabledForTemplateDeployment?: boolean;
  enablePurgeProtection?: boolean;
  enableSoftDelete?: boolean;
  environmentSettings?: KeyVaultEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateKeyVaultRequest {
  name: string;
  location: string;
  enableRbacAuthorization?: boolean;
  enabledForDeployment?: boolean;
  enabledForDiskEncryption?: boolean;
  enabledForTemplateDeployment?: boolean;
  enablePurgeProtection?: boolean;
  enableSoftDelete?: boolean;
  environmentSettings?: KeyVaultEnvironmentConfigEntry[];
}
