// ─── Responses ───────────────────────────────────────────────────────────────

export interface CrossConfigReferenceResponse {
  referenceId: string;
  targetResourceId: string;
  targetResourceName: string;
  targetResourceType: string;
  targetResourceGroupName: string;
  targetConfigId: string;
  targetConfigName: string;
}

export interface IncomingCrossConfigReferenceResponse {
  referenceId: string;
  sourceConfigId: string;
  sourceConfigName: string;
  sourceResourceId: string;
  sourceResourceName: string;
  sourceResourceType: string;
  sourceResourceGroupName: string;
  targetResourceId: string;
  targetResourceName: string;
  targetResourceType: string;
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
}
