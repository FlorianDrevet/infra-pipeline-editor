export type RepositoryContentKind = 'Infrastructure' | 'ApplicationCode';

export type ProjectLayoutPreset = 'AllInOne' | 'SplitInfraCode' | 'MultiRepo';

export interface ProjectRepositoryResponse {
  id: string;
  alias: string;
  providerType: string;
  repositoryUrl: string;
  owner: string;
  repositoryName: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface AddProjectRepositoryRequest {
  alias: string;
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface UpdateProjectRepositoryRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface SetProjectLayoutPresetRequest {
  preset: ProjectLayoutPreset;
}
