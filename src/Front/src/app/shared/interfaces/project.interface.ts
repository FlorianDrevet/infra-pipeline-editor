import {
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
  TagRequest,
  TagResponse,
} from './infra-config.interface';

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
  repositoryMode: string;
  members: ProjectMemberResponse[];
  environmentDefinitions: EnvironmentDefinitionResponse[];
  defaultNamingTemplate: string | null;
  resourceNamingTemplates: ResourceNamingTemplateResponse[];
  gitRepositoryConfiguration?: GitConfigResponse | null;
  tags: TagResponse[];
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
  repositoryMode?: string;
}

export interface SetProjectTagsRequest {
  tags: TagRequest[];
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

// ─── Git Configuration ──────────────────────────────────────────────────────


export interface GitConfigResponse {
  id: string;
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  basePath?: string | null;
  pipelineBasePath?: string | null;
  owner: string;
  repositoryName: string;
}

export interface SetGitConfigRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  basePath?: string | null;
  pipelineBasePath?: string | null;
  personalAccessToken: string;
}

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

// ─── Repository Mode ────────────────────────────────────────────────────────

export interface SetRepositoryModeRequest {
  repositoryMode: string;
}

export interface GenerateProjectBicepResponse {
  commonFileUris: Record<string, string>;
  configFileUris: Record<string, Record<string, string>>;
}

export interface GenerateProjectPipelineResponse {
  commonFileUris: Record<string, string>;
  configFileUris: Record<string, Record<string, string>>;
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
