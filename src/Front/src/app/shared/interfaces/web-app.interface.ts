import { AcrAuthMode } from './container-registry.interface';
import { PipelineStepOptions } from '../../features/resource-edit/models/pipeline-step-options.model';

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
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName: string | null;
  dockerfilePath: string | null;
  sourceCodePath: string | null;
  buildCommand: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
  environmentSettings: WebAppEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateWebAppRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  appServicePlanId: string;
  isExisting?: boolean;
  deploymentMode?: string;
  containerRegistryId?: string | null;
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  sourceCodePath?: string | null;
  buildCommand?: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName?: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
}

export interface UpdateWebAppRequest {
  name: string;
  location: string;
  appServicePlanId: string;
  deploymentMode?: string;
  containerRegistryId?: string | null;
  acrAuthMode?: AcrAuthMode | null;
  dockerImageName?: string | null;
  dockerfilePath?: string | null;
  sourceCodePath?: string | null;
  buildCommand?: string | null;
  runtimeStack: string;
  runtimeVersion: string;
  alwaysOn: boolean;
  httpsOnly: boolean;
  applicationName?: string | null;
  pipelineStepOptions?: PipelineStepOptions | null;
  environmentSettings?: WebAppEnvironmentConfigEntry[];
}
