export type MultiRepoPushMode = 'both' | 'infra' | 'code';

export interface RepoPushTarget {
  repositoryId: string;
  branchName: string;
  commitMessage: string;
}

export interface MultiRepoPushRequest {
  infra?: RepoPushTarget;
  code?: RepoPushTarget;
}

export interface RepoPushResult {
  repositoryId: string;
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
