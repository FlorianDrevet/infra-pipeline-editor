export interface GenerateBicepRequest {
  infrastructureConfigId: string;
}

export interface GenerateBicepResponse {
  mainBicepUri: string;
  parameterFileUris: Record<string, string>;
  moduleUris: Record<string, string>;
}