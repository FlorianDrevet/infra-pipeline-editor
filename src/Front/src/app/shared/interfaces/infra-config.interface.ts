// ─── Responses ───────────────────────────────────────────────────────────────

export interface TagResponse {
  name: string;
  value: string;
}

export interface MemberResponse {
  id: string;
  userId: string;
  entraId: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface UserResponse {
  id: string;
  firstName: string;
  lastName: string;
}

export interface EnvironmentDefinitionResponse {
  id: string;
  name: string;
  shortName: string;
  prefix: string;
  suffix: string;
  location: string;
  tenantId: string;
  subscriptionId: string;
  order: number;
  requiresApproval: boolean;
  tags: TagResponse[];
}

export interface ResourceNamingTemplateResponse {
  id: string;
  resourceType: string;
  template: string;
}

export interface InfrastructureConfigResponse {
  id: string;
  name: string;
  defaultNamingTemplate: string | null;
  projectId: string;
  useProjectNamingConventions: boolean;
  resourceNamingTemplates: ResourceNamingTemplateResponse[];
  resourceGroupCount: number;
  resourceCount: number;
  crossConfigReferenceCount: number;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateInfrastructureConfigRequest {
  name: string;
  projectId: string;
}

export interface TagRequest {
  name: string;
  value: string;
}

export interface AddMemberRequest {
  userId: string;
  role: string;
}

export interface UpdateMemberRoleRequest {
  newRole: string;
}

export interface SetDefaultNamingTemplateRequest {
  template: string | null;
}

export interface SetResourceNamingTemplateRequest {
  template: string;
}
