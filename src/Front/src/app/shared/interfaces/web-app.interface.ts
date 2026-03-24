// ─── Environment Settings ────────────────────────────────────────────────────

export interface WebAppEnvironmentConfigEntry {
  environmentName: string;
  alwaysOn?: boolean | null;
  httpsOnly?: boolean | null;
  runtimeStack?: string | null;
  runtimeVersion?: string | null;
}

export interface WebAppEnvironmentConfigResponse {
  environmentName: string;
  alwaysOn: boolean | null;
  httpsOnly: boolean | null;
  runtimeStack: string | null;
  runtimeVersion: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface WebAppResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  environmentSettings: WebAppEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateWebAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
}

export interface UpdateWebAppRequest {
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
}
