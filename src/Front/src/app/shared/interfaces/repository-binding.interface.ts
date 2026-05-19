export interface RepositoryBindingResponse {
  repositoryId: string;
  branch: string | null;
  infraPath: string | null;
  pipelinePath: string | null;
}

export interface SetInfraConfigRepositoryBindingRequest {
  repositoryId: string | null;
  branch: string | null;
  infraPath: string | null;
  pipelinePath: string | null;
}
