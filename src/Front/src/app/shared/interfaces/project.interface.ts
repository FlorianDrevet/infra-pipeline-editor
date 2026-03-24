import {
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
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
  members: ProjectMemberResponse[];
  environmentDefinitions: EnvironmentDefinitionResponse[];
  defaultNamingTemplate: string | null;
  resourceNamingTemplates: ResourceNamingTemplateResponse[];
  gitRepositoryConfiguration?: GitConfigResponse | null;
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
}

export interface AddProjectMemberRequest {
  userId: string;
  role: string;
}

export interface UpdateProjectMemberRoleRequest {
  newRole: string;
}

// ─── Git Configuration ──────────────────────────────────────────────────────


export interface GitConfigResponse {
  id: string;
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  basePath?: string | null;
  owner: string;
  repositoryName: string;
}

export interface SetGitConfigRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  basePath?: string | null;
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
