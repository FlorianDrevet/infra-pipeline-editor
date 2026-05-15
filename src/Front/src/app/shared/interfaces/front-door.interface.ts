// ─── Environment Settings ────────────────────────────────────────────────────

export interface FrontDoorEnvironmentConfigResponse {
  environmentName: string;
  sku: string;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface FrontDoorOriginResponse {
  id: string;
  targetResourceId: string;
  hostName: string | null;
  privateLinkEnabled: boolean;
  weight: number;
  priority: number;
}

export interface FrontDoorResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  wafPolicyEnabled: boolean;
  origins: FrontDoorOriginResponse[];
  environmentSettings: FrontDoorEnvironmentConfigResponse[];
  isExisting: boolean;
}
