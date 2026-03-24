// ─── Environment Settings ────────────────────────────────────────────────────

export interface AppConfigurationEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  softDeleteRetentionInDays?: number | null;
  purgeProtectionEnabled?: boolean | null;
  disableLocalAuth?: boolean | null;
  publicNetworkAccess?: string | null;
}

export interface AppConfigurationEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  softDeleteRetentionInDays: number | null;
  purgeProtectionEnabled: boolean | null;
  disableLocalAuth: boolean | null;
  publicNetworkAccess: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppConfigurationResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: AppConfigurationEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateAppConfigurationRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: AppConfigurationEnvironmentConfigEntry[];
}

export interface UpdateAppConfigurationRequest {
  name: string;
  location: string;
  environmentSettings?: AppConfigurationEnvironmentConfigEntry[];
}
