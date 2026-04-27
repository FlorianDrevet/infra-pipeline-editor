// ─── Environment Settings ────────────────────────────────────────────────────

export interface LogAnalyticsWorkspaceEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  retentionInDays?: number | null;
  dailyQuotaGb?: number | null;
}

export interface LogAnalyticsWorkspaceEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  retentionInDays: number | null;
  dailyQuotaGb: number | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface LogAnalyticsWorkspaceResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: LogAnalyticsWorkspaceEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateLogAnalyticsWorkspaceRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: LogAnalyticsWorkspaceEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateLogAnalyticsWorkspaceRequest {
  name: string;
  location: string;
  environmentSettings?: LogAnalyticsWorkspaceEnvironmentConfigEntry[];
}
