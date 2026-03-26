// ─── Responses ───────────────────────────────────────────────────────────────

export interface CrossConfigReferenceResponse {
  id: string;
  targetResourceId: string;
  targetResourceName: string;
  targetResourceType: string;
  targetResourceGroupName: string;
  targetConfigId: string;
  targetConfigName: string;
  alias: string;
  purpose: string | null;
}

export interface ProjectResourceResponse {
  resourceId: string;
  resourceName: string;
  resourceType: string;
  resourceGroupName: string;
  configId: string;
  configName: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddCrossConfigReferenceRequest {
  targetResourceId: string;
  alias: string;
  purpose?: string;
}
