export interface ProjectMultiRepoPushRepositoryTarget {
  repositoryId: string;
  branchName: string;
  commitMessage: string;
}

export interface ProjectMultiRepoPushConfigurationTarget {
  infrastructureConfigId: string;
  repositories: ProjectMultiRepoPushRepositoryTarget[];
}

export interface ProjectMultiRepoPushRequest {
  configurations: ProjectMultiRepoPushConfigurationTarget[];
}

export interface ProjectMultiRepoPushResult {
  infrastructureConfigId: string;
  repositoryId: string;
  success: boolean;
  branchUrl: string | null;
  commitSha: string | null;
  fileCount: number;
  errorCode: string | null;
  errorDescription: string | null;
}

export interface ProjectMultiRepoPushResponse {
  results: ProjectMultiRepoPushResult[];
}