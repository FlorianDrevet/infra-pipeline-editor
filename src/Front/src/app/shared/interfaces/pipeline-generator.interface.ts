export interface GeneratePipelineRequest {
  infrastructureConfigId: string;
}

export interface GeneratePipelineResponse {
  fileUris: Record<string, string>;
}

// ─── App Pipeline ────────────────────────────────────────────────────────────

export interface GenerateAppPipelineRequest {
  infrastructureConfigId: string;
  resourceId: string;
}

export interface GenerateAppPipelineResponse {
  fileUris: Record<string, string>;
}

// ─── Push to Git ─────────────────────────────────────────────────────────────

export interface PushPipelineToGitRequest {
  branchName: string;
  commitMessage: string;
}

export interface PushPipelineToGitResponse {
  branchName: string;
  branchUrl: string;
  commitSha: string;
  fileCount: number;
}
