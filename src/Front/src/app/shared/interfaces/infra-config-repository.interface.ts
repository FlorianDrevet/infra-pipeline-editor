import { RepositoryContentKind } from './project-repository.interface';

export type ConfigLayoutMode = 'AllInOne' | 'SplitInfraCode';

export interface InfraConfigRepositoryResponse {
  id: string;
  providerType: string | null;
  repositoryUrl: string | null;
  owner: string | null;
  repositoryName: string | null;
  defaultBranch: string | null;
  contentKinds: RepositoryContentKind[];
}

export interface AddInfraConfigRepositoryRequest {
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
