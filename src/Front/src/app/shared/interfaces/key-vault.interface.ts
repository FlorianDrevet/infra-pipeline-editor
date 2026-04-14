// ─── Environment Settings ────────────────────────────────────────────────────

export interface KeyVaultEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
}

export interface KeyVaultEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
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
