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
  environmentSettings: KeyVaultEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateKeyVaultRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: KeyVaultEnvironmentConfigEntry[];
}

export interface UpdateKeyVaultRequest {
  name: string;
  location: string;
  environmentSettings?: KeyVaultEnvironmentConfigEntry[];
}
