// ─── Environment Settings ────────────────────────────────────────────────────

export interface DocumentIntelligenceEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  publicNetworkAccess?: string | null;
  disableLocalAuth?: boolean | null;
}

export interface DocumentIntelligenceEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  publicNetworkAccess: string | null;
  disableLocalAuth: boolean | null;
  isExisting?: boolean;
}

// ─── Response ────────────────────────────────────────────────────────────────

export interface DocumentIntelligenceResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  customSubDomainName: string | null;
  environmentSettings: DocumentIntelligenceEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateDocumentIntelligenceRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  customSubDomainName?: string | null;
  environmentSettings?: DocumentIntelligenceEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateDocumentIntelligenceRequest {
  name: string;
  location: string;
  customSubDomainName?: string | null;
  environmentSettings?: DocumentIntelligenceEnvironmentConfigEntry[];
}
