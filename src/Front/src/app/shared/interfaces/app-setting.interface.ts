// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppSettingResponse {
  id: string;
  resourceId: string;
  name: string;
  environmentValues?: Record<string, string> | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
  isOutputReference: boolean;
  keyVaultResourceId?: string | null;
  secretName?: string | null;
  isKeyVaultReference: boolean;
  hasKeyVaultAccess?: boolean | null;
  secretValueAssignment?: string | null;
  variableGroupId: string | null;
  pipelineVariableName: string | null;
  variableGroupName: string | null;
  isViaVariableGroup: boolean;
}

export interface AvailableOutputsResponse {
  resourceTypeName: string;
  outputs: OutputDefinitionResponse[];
}

export interface OutputDefinitionResponse {
  name: string;
  description: string;
  isSensitive: boolean;
}

export interface CheckKeyVaultAccessResponse {
  hasAccess: boolean;
  missingRoleDefinitionId?: string | null;
  missingRoleName?: string | null;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddAppSettingRequest {
  name: string;
  environmentValues?: Record<string, string> | null;
  sourceResourceId?: string | null;
  sourceOutputName?: string | null;
  keyVaultResourceId?: string | null;
  secretName?: string | null;
  exportToKeyVault?: boolean;
  secretValueAssignment?: string | null;
  variableGroupId?: string;
  pipelineVariableName?: string;
}

export interface UpdateStaticAppSettingRequest {
  name: string;
  environmentValues: Record<string, string>;
}
