// ─── Environment Settings ────────────────────────────────────────────────────

export interface AppServicePlanEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  capacity?: number | null;
}

export interface AppServicePlanEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  capacity: number | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppServicePlanResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  osType: string;
  environmentSettings: AppServicePlanEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateAppServicePlanRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  osType: string;
  environmentSettings?: AppServicePlanEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateAppServicePlanRequest {
  name: string;
  location: string;
  osType: string;
  environmentSettings?: AppServicePlanEnvironmentConfigEntry[];
}
