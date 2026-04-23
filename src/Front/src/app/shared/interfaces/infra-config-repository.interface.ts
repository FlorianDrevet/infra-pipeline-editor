import { RepositoryContentKind } from './project-repository.interface';

export type ConfigLayoutMode = 'AllInOne' | 'SplitInfraCode';

export interface InfraConfigRepositoryResponse {
  id: string;
  alias: string;
  providerType: string;
  repositoryUrl: string;
  owner: string;
  repositoryName: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface AddInfraConfigRepositoryRequest {
  alias: string;
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface UpdateInfraConfigRepositoryRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

export interface SetInfraConfigLayoutModeRequest {
  mode: ConfigLayoutMode | null;
}
