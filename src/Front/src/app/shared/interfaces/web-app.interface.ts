// ─── Environment Settings ────────────────────────────────────────────────────

export interface WebAppEnvironmentConfigEntry {
  environmentName: string;
  alwaysOn?: boolean | null;
  httpsOnly?: boolean | null;
  dockerImageTag?: string | null;
}

export interface WebAppEnvironmentConfigResponse {
  environmentName: string;
  alwaysOn: boolean | null;
  httpsOnly: boolean | null;
  dockerImageTag: string | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface WebAppResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  deploymentMode: string;
  containerRegistryId: string | null;
  dockerImageName: string | null;
  dockerfilePath: string | null;
  sourceCodePath: string | null;
  buildCommand: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName: string | null;
  environmentSettings: WebAppEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateWebAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  deploymentMode?: string;
  containerRegistryId?: string | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  sourceCodePath?: string | null;
  buildCommand?: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName?: string | null;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateWebAppRequest {
  name: string;
  location: string;
  appServicePlanId: string;
  deploymentMode?: string;
  containerRegistryId?: string | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  sourceCodePath?: string | null;
  buildCommand?: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName?: string | null;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
}
