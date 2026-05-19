export type RepositoryContentKind = 'Infrastructure' | 'ApplicationCode';

export type ProjectLayoutPreset = 'AllInOne' | 'SplitInfraCode' | 'MultiRepo';

export interface ProjectRepositoryResponse {
  id: string;
  providerType: string | null;
  repositoryUrl: string | null;
  owner: string | null;
  repositoryName: string | null;
  defaultBranch: string | null;
  isConfigured: boolean;
  contentKinds: RepositoryContentKind[];
}

export interface AddProjectRepositoryRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  personalAccessToken: string;
  contentKinds: RepositoryContentKind[];
}

export interface UpdateProjectRepositoryRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  personalAccessToken?: string;
  contentKinds: RepositoryContentKind[];
}

export interface VerifyProjectRepositoryRequest {
  providerType: string;
  repositoryUrl: string;
  personalAccessToken?: string;
}

export interface VerifiedGitBranchResponse {
  name: string;
  isProtected: boolean;
}

export interface VerifyProjectRepositoryResponse {
  owner: string | null;
  repositoryName: string | null;
  branches: VerifiedGitBranchResponse[];
  defaultBranchCandidate: string | null;
}

export interface SetProjectLayoutPresetRequest {
  preset: ProjectLayoutPreset;
}
