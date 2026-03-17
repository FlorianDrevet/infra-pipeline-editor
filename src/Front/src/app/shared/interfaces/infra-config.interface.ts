// ─── Responses ───────────────────────────────────────────────────────────────

export interface TagResponse {
  name: string;
  value: string;
}

export interface MemberResponse {
  id: string;
  userId: string;
  role: string;
}

export interface EnvironmentDefinitionResponse {
  id: string;
  name: string;
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
  members: MemberResponse[];
  environmentDefinitions: EnvironmentDefinitionResponse[];
  resourceNamingTemplates: ResourceNamingTemplateResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateInfrastructureConfigRequest {
  name: string;
}

export interface TagRequest {
  name: string;
  value: string;
}

export interface AddEnvironmentRequest {
  name: string;
  prefix?: string;
  suffix?: string;
  location: string;
  tenantId: string;
  subscriptionId: string;
  order?: number;
  requiresApproval?: boolean;
  tags?: TagRequest[];
}

export interface UpdateEnvironmentRequest {
  name: string;
  prefix?: string;
  suffix?: string;
  location: string;
  tenantId: string;
  subscriptionId: string;
  order?: number;
  requiresApproval?: boolean;
  tags?: TagRequest[];
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
