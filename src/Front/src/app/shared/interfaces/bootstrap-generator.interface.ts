export interface GenerateBootstrapRequest {
  infrastructureConfigId: string;
}

export interface GenerateBootstrapResponse {
  fileUris: Record<string, string>;
}

// ─── Push to Git ─────────────────────────────────────────────────────────────

export interface PushBootstrapToGitRequest {
  branchName: string;
  commitMessage: string;
}

export interface PushBootstrapToGitResponse {
  branchName: string;
  branchUrl: string;
  commitSha: string;
  fileCount: number;
}
