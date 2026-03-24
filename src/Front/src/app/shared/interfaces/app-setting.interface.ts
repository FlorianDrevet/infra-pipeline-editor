// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppSettingResponse {
  id: string;
  resourceId: string;
  name: string;
  staticValue?: string | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
  isOutputReference: boolean;
}

export interface AvailableOutputsResponse {
  resourceTypeName: string;
  outputs: OutputDefinitionResponse[];
}

export interface OutputDefinitionResponse {
  name: string;
  description: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddAppSettingRequest {
  name: string;
  staticValue?: string | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
}
