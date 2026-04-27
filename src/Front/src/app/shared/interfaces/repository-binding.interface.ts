export interface RepositoryBindingResponse {
  alias: string;
  branch: string | null;
  infraPath: string | null;
  pipelinePath: string | null;
}

export interface SetInfraConfigRepositoryBindingRequest {
  repositoryAlias: string | null;
  branch: string | null;
  infraPath: string | null;
  pipelinePath: string | null;
}
