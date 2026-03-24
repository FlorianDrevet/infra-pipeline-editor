// ─── Environment Settings ────────────────────────────────────────────────────

export interface FunctionAppEnvironmentConfigEntry {
  environmentName: string;
  httpsOnly?: boolean | null;
  runtimeStack?: string | null;
  runtimeVersion?: string | null;
  maxInstanceCount?: number | null;
  functionsWorkerRuntime?: string | null;
}

export interface FunctionAppEnvironmentConfigResponse {
  environmentName: string;
  httpsOnly: boolean | null;
  runtimeStack: string | null;
  runtimeVersion: string | null;
  maxInstanceCount: number | null;
  functionsWorkerRuntime: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface FunctionAppResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  httpsOnly: boolean;
  environmentSettings: FunctionAppEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateFunctionAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  httpsOnly: boolean;
  environmentSettings?: FunctionAppEnvironmentConfigEntry[];
}

export interface UpdateFunctionAppRequest {
  name: string;
  location: string;
  appServicePlanId: string;
  runtimeStack: string;
  runtimeVersion: string;
  httpsOnly: boolean;
  environmentSettings?: FunctionAppEnvironmentConfigEntry[];
}
