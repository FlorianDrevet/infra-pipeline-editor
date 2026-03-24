// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppSettingResponse {
  id: string;
  resourceId: string;
  name: string;
  staticValue?: string | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
  isOutputReference: boolean;
  keyVaultResourceId?: string | null;
  secretName?: string | null;
  isKeyVaultReference: boolean;
  hasKeyVaultAccess?: boolean | null;
}

export interface AvailableOutputsResponse {
  resourceTypeName: string;
  outputs: OutputDefinitionResponse[];
}

export interface OutputDefinitionResponse {
  name: string;
  description: string;
}

export interface CheckKeyVaultAccessResponse {
  hasAccess: boolean;
  missingRoleDefinitionId?: string | null;
  missingRoleName?: string | null;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddAppSettingRequest {
  name: string;
  staticValue?: string | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
  keyVaultResourceId?: string | null;
  secretName?: string | null;
}
