import { AcrAuthMode } from './container-registry.interface';
import { PipelineStepOptions } from '../../features/resource-edit/models/pipeline-step-options.model';

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
  containerRegistryServiceConnection?: string | null;
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
  containerRegistryServiceConnection?: string | null;
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
  acrPullIdentityId?: string | null;
  dockerImageName: string | null;
  dockerImageValidated: boolean;
  dockerfilePath: string | null;
  applicationName: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
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
  acrPullIdentityId?: string | null;
  dockerImageName?: string | null;
  dockerImageValidated?: boolean;
  dockerfilePath?: string | null;
  applicationName?: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateContainerAppRequest {
  name: string;
  location: string;
  containerAppEnvironmentId: string;
  containerRegistryId?: string | null;
  acrAuthMode?: AcrAuthMode | null;
  acrPullIdentityId?: string | null;
  dockerImageName?: string | null;
  dockerImageValidated?: boolean;
  dockerfilePath?: string | null;
  applicationName?: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
  environmentSettings?: ContainerAppEnvironmentConfigEntry[];
}
