// ─── Environment Settings ────────────────────────────────────────────────────

export interface ApplicationInsightsEnvironmentConfigEntry {
  environmentName: string;
  samplingPercentage?: number | null;
  retentionInDays?: number | null;
  disableIpMasking?: boolean | null;
  disableLocalAuth?: boolean | null;
  ingestionMode?: string | null;
}

export interface ApplicationInsightsEnvironmentConfigResponse {
  environmentName: string;
  samplingPercentage: number | null;
  retentionInDays: number | null;
  disableIpMasking: boolean | null;
  disableLocalAuth: boolean | null;
  ingestionMode: string | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ApplicationInsightsResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  logAnalyticsWorkspaceId: string;
  environmentSettings: ApplicationInsightsEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateApplicationInsightsRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  logAnalyticsWorkspaceId: string;
  environmentSettings?: ApplicationInsightsEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateApplicationInsightsRequest {
  name: string;
  location: string;
  logAnalyticsWorkspaceId: string;
  environmentSettings?: ApplicationInsightsEnvironmentConfigEntry[];
}
