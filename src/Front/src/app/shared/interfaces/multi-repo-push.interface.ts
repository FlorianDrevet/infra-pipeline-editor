export interface RepoPushTarget {
  alias: string;
  branchName: string;
  commitMessage: string;
}

export interface MultiRepoPushRequest {
  infra: RepoPushTarget;
  code: RepoPushTarget;
}

export interface RepoPushResult {
  alias: string;
  success: boolean;
  branchUrl: string | null;
  commitSha: string | null;
  fileCount: number;
  errorCode: string | null;
  errorDescription: string | null;
}

export interface MultiRepoPushResponse {
  results: RepoPushResult[];
}
