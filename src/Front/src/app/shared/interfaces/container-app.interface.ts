import { AcrAuthMode } from './container-registry.interface';

// ─── Environment Settings ────────────────────────────────────────────────────

export interface ContainerAppEnvironmentConfigEntry {
  environmentName: string;
  cpuCores?: string | null;
  memoryGi?: string | null;
  minReplicas?: number | null;
  maxReplicas?: number | null;
  ingressEnabled?: boolean | null;
  ingressTargetPort?: number | null;
  ingressExternal?: boolean | null;
  transportMethod?: string | null;
  readinessProbePath?: string | null;
  readinessProbePort?: number | null;
  livenessProbePath?: string | null;
  livenessProbePort?: number | null;
  startupProbePath?: string | null;
  startupProbePort?: number | null;
}

export interface ContainerAppEnvironmentConfigResponse {
  environmentName: string;
  cpuCores: string | null;
  memoryGi: string | null;
  minReplicas: number | null;
  maxReplicas: number | null;
  ingressEnabled: boolean | null;
  ingressTargetPort: number | null;
  ingressExternal: boolean | null;
  transportMethod: string | null;
  readinessProbePath: string | null;
  readinessProbePort: number | null;
  livenessProbePath: string | null;
  livenessProbePort: number | null;
  startupProbePath: string | null;
  startupProbePort: number | null;
  isExisting?: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ContainerAppResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  containerRegistryId: string | null;
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName: string | null;
  dockerfilePath: string | null;
  applicationName: string | null;
  environmentSettings: ContainerAppEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateContainerAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  containerRegistryId?: string | null;
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  applicationName?: string | null;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateContainerAppRequest {
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  containerRegistryId?: string | null;
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  applicationName?: string | null;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
}
