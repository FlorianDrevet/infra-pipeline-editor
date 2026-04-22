// ─── Environment Settings ────────────────────────────────────────────────────

export interface FunctionAppEnvironmentConfigEntry {
  environmentName: string;
  httpsOnly?: boolean | null;
  maxInstanceCount?: number | null;
  dockerImageTag?: string | null;
}

export interface FunctionAppEnvironmentConfigResponse {
  environmentName: string;
  httpsOnly: boolean | null;
  maxInstanceCount: number | null;
  dockerImageTag: string | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface FunctionAppResponse {
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
  httpsOnly: boolean;
  applicationName: string | null;
  environmentSettings: FunctionAppEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateFunctionAppRequest {
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
  httpsOnly: boolean;
  applicationName?: string | null;
  environmentSettings?: FunctionAppEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateFunctionAppRequest {
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
  httpsOnly: boolean;
  applicationName?: string | null;
  environmentSettings?: FunctionAppEnvironmentConfigEntry[];
}
