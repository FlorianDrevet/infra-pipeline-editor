export interface GenerateBicepRequest {
  infrastructureConfigId: string;
}

export interface GenerateBicepResponse {
  mainBicepUri: string;
  parameterFileUris: Record<string, string>;
  moduleUris: Record<string, string>;
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