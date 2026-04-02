// ─── Constants ───────────────────────────────────────────────────────────────

/** Well-known Azure role definition ID for AcrPull. Must use User Assigned Identity only. */
export const ACR_PULL_ROLE_DEFINITION_ID = '7f951dda-4ed3-4680-a7ca-43fe172d538e';

// ─── Responses ───────────────────────────────────────────────────────────────

export interface RoleAssignmentsWithIdentityResponse {
  assignedUserAssignedIdentityId: string | null;
  assignedUserAssignedIdentityName: string | null;
  roleAssignments: RoleAssignmentResponse[];
}

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

export interface AssignIdentityToResourceRequest {
  userAssignedIdentityId: string;
}

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
