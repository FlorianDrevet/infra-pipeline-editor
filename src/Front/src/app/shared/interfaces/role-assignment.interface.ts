// ─── Responses ───────────────────────────────────────────────────────────────

export interface RoleAssignmentResponse {
  id: string;
  sourceResourceId: string;
  targetResourceId: string;
  managedIdentityType: string;
  roleDefinitionId: string;
  userAssignedIdentityId?: string;
}

export interface AzureRoleDefinitionResponse {
  id: string;
  name: string;
  description: string;
  documentationUrl: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddRoleAssignmentRequest {
  targetResourceId: string;
  managedIdentityType: string;
  roleDefinitionId: string;
  userAssignedIdentityId?: string;
}

export interface UpdateRoleAssignmentIdentityRequest {
  managedIdentityType: string;
  userAssignedIdentityId?: string;
}

export interface IdentityRoleAssignmentResponse {
  id: string;
  sourceResourceId: string;
  sourceResourceName: string;
  sourceResourceType: string;
  targetResourceId: string;
  targetResourceName: string;
  targetResourceType: string;
  roleDefinitionId: string;
  roleName: string;
}

// ─── Impact Analysis ─────────────────────────────────────────────────────────

export interface RoleAssignmentImpactItemResponse {
  affectedResourceId: string;
  affectedResourceName: string;
  affectedResourceType: string;
  targetResourceId: string;
  targetResourceName: string;
  targetResourceType: string;
  impactType: string;
  description: string;
  severity: string;
  affectedSettingsCount: number | null;
}

export interface RoleAssignmentImpactResponse {
  hasImpact: boolean;
  impacts: RoleAssignmentImpactItemResponse[];
}
