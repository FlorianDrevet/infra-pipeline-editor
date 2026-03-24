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
