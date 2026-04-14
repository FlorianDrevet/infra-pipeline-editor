// ─── Environment Settings ────────────────────────────────────────────────────

export interface ContainerAppEnvironmentEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  workloadProfileType?: string | null;
  internalLoadBalancerEnabled?: boolean | null;
  zoneRedundancyEnabled?: boolean | null;
}

export interface ContainerAppEnvironmentEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  workloadProfileType: string | null;
  internalLoadBalancerEnabled: boolean | null;
  zoneRedundancyEnabled: boolean | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ContainerAppEnvironmentResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  logAnalyticsWorkspaceId: string | null;
  environmentSettings: ContainerAppEnvironmentEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateContainerAppEnvironmentRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  logAnalyticsWorkspaceId?: string | null;
  environmentSettings?: ContainerAppEnvironmentEnvironmentConfigEntry[];
}

export interface UpdateContainerAppEnvironmentRequest {
  name: string;
  location: string;
  logAnalyticsWorkspaceId?: string | null;
  environmentSettings?: ContainerAppEnvironmentEnvironmentConfigEntry[];
}
