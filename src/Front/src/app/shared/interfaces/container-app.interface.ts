// ─── Environment Settings ────────────────────────────────────────────────────

export interface ContainerAppEnvironmentConfigEntry {
  environmentName: string;
  containerImage?: string | null;
  cpuCores?: string | null;
  memoryGi?: string | null;
  minReplicas?: number | null;
  maxReplicas?: number | null;
  ingressEnabled?: boolean | null;
  ingressTargetPort?: number | null;
  ingressExternal?: boolean | null;
  transportMethod?: string | null;
}

export interface ContainerAppEnvironmentConfigResponse {
  environmentName: string;
  containerImage: string | null;
  cpuCores: string | null;
  memoryGi: string | null;
  minReplicas: number | null;
  maxReplicas: number | null;
  ingressEnabled: boolean | null;
  ingressTargetPort: number | null;
  ingressExternal: boolean | null;
  transportMethod: string | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ContainerAppResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  environmentSettings: ContainerAppEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateContainerAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
}

export interface UpdateContainerAppRequest {
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
}
