import {
  EnvironmentDefinitionResponse,
  ResourceAbbreviationOverrideResponse,
  ResourceNamingTemplateResponse,
  TagRequest,
  TagResponse,
} from './infra-config.interface';
import { ProjectRepositoryResponse } from './project-repository.interface';

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ProjectMemberResponse {
  id: string;
  userId: string;
  entraId: string;
  role: string;
  firstName: string;
  lastName: string;
}

export interface ProjectResponse {
  id: string;
  name: string;
  description?: string;
  members: ProjectMemberResponse[];
  environmentDefinitions: EnvironmentDefinitionResponse[];
  defaultNamingTemplate: string | null;
  resourceNamingTemplates: ResourceNamingTemplateResponse[];
  resourceAbbreviations: ResourceAbbreviationOverrideResponse[];
  tags: TagResponse[];
  agentPoolName: string | null;
  usedResourceTypes?: string[];
  repositories?: ProjectRepositoryResponse[];
  layoutPreset?: string;
}

export interface RecentItemResponse {
  id: string;
  name: string;
  type: 'project' | 'config';
  description?: string;
}

export interface ValidateRecentItemsRequest {
  items: { id: string; type: string }[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateProjectRequest {
  name: string;
  description?: string;
  isExisting?: boolean;
}

export interface SetProjectTagsRequest {
  tags: TagRequest[];
}

export interface SetAgentPoolRequest {
  agentPoolName: string | null;
}

export interface AddProjectMemberRequest {
  userId: string;
  role: string;
}

export interface UpdateProjectMemberRoleRequest {
  newRole: string;
}

// ─── Environment Requests ───────────────────────────────────────────────────

export interface AddProjectEnvironmentRequest {
  name: string;
  shortName?: string;
  prefix?: string;
  suffix?: string;
  location: string;
  subscriptionId: string;
  order?: number;
  requiresApproval?: boolean;
  azureResourceManagerConnection?: string;
  tags?: TagRequest[];
}

export interface UpdateProjectEnvironmentRequest {
  name: string;
  shortName?: string;
  prefix?: string;
  suffix?: string;
  location: string;
  subscriptionId: string;
  order?: number;
  requiresApproval?: boolean;
  azureResourceManagerConnection?: string;
  tags?: TagRequest[];
}

// ─── Git Operations ──────────────────────────────────────────────────────


export interface TestGitConnectionResponse {
  success: boolean;
  repositoryFullName?: string | null;
  defaultBranch?: string | null;
  errorMessage?: string | null;
}

export interface GitBranchResponse {
  name: string;
  isProtected: boolean;
}

export interface GenerateProjectBicepResponse {
  commonFileUris: Record<string, string>;
  configFileUris: Record<string, Record<string, string>>;
}

export interface GenerateProjectPipelineResponse {
  commonFileUris: Record<string, string>;
  configFileUris: Record<string, Record<string, string>>;
  infraCommonFileUris: Record<string, string>;
  appCommonFileUris: Record<string, string>;
  infraConfigFileUris: Record<string, Record<string, string>>;
  appConfigFileUris: Record<string, Record<string, string>>;
}

export interface GenerateProjectBootstrapPipelineResponse {
  fileUris: Record<string, string>;
}

// ─── Project Pipeline Variable Groups ────────────────────────────────────────

export interface PipelineVariableUsageResponse {
  pipelineVariableName: string;
  appSettingName: string;
  resourceName: string;
  resourceType: string;
  configName: string;
}

export interface ProjectPipelineVariableGroupResponse {
  id: string;
  groupName: string;
  variables: PipelineVariableUsageResponse[];
}

export interface AddProjectPipelineVariableGroupRequest {
  groupName: string;
}
