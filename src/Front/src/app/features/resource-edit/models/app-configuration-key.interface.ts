// ─── Responses ───────────────────────────────────────────────────────────────

export interface AppConfigurationKeyResponse {
  id: string;
  appConfigurationId: string;
  key: string;
  label: string | null;
  environmentValues: Record<string, string> | null;
  keyVaultResourceId: string | null;
  secretName: string | null;
  isKeyVaultReference: boolean;
  hasKeyVaultAccess: boolean | null;
  secretValueAssignment: string | null;
  variableGroupId: string | null;
  pipelineVariableName: string | null;
  variableGroupName: string | null;
  isViaVariableGroup: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddAppConfigurationKeyRequest {
  key: string;
  label?: string;
  environmentValues?: Record<string, string>;
  keyVaultResourceId?: string;
  secretName?: string;
  secretValueAssignment?: string;
  variableGroupId?: string;
  pipelineVariableName?: string;
}
