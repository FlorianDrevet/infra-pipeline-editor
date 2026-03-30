// ─── Environment Settings ────────────────────────────────────────────────────

export interface ContainerRegistryEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  adminUserEnabled?: boolean | null;
  publicNetworkAccess?: string | null;
  zoneRedundancy?: boolean | null;
}

export interface ContainerRegistryEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  adminUserEnabled: boolean | null;
  publicNetworkAccess: string | null;
  zoneRedundancy: boolean | null;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface ContainerRegistryResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: ContainerRegistryEnvironmentConfigResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateContainerRegistryRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: ContainerRegistryEnvironmentConfigEntry[];
}

export interface UpdateContainerRegistryRequest {
  name: string;
  location: string;
  environmentSettings?: ContainerRegistryEnvironmentConfigEntry[];
}

// ─── ACR Access Check ────────────────────────────────────────────────────────

export interface CheckAcrPullAccessResponse {
  hasAccess: boolean;
  missingRoleDefinitionId?: string;
  missingRoleName?: string;
  assignedUserAssignedIdentityId?: string;
  assignedUserAssignedIdentityName?: string;
  hasUserAssignedIdentity: boolean;
}
