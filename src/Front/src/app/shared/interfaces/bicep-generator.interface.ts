export interface GenerateBicepRequest {
  infrastructureConfigId: string;
}

export interface ResourceDiagnosticResponse {
  resourceId: string;
  resourceName: string;
  resourceType: string;
  severity: string;
  ruleCode: string;
  targetResourceName: string;
}

export interface GenerateBicepResponse {
  mainBicepUri: string;
  constantsBicepUri?: string | null;
  parameterFileUris: Record<string, string>;
  moduleUris: Record<string, string>;
  warnings?: ResourceDiagnosticResponse[];
}

// ─── Push to Git ─────────────────────────────────────────────────────────────

export interface PushBicepToGitRequest {
  branchName: string;
  commitMessage?: string | null;
}

export interface PushBicepToGitResponse {
  branchName: string;
  branchUrl: string;
  commitSha: string;
  fileCount: number;
}