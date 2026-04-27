export interface SecureParameterMappingResponse {
  id: string;
  secureParameterName: string;
  variableGroupId: string | null;
  variableGroupName: string | null;
  pipelineVariableName: string | null;
}

export interface SetSecureParameterMappingRequest {
  secureParameterName: string;
  variableGroupId: string | null;
  pipelineVariableName: string | null;
}
