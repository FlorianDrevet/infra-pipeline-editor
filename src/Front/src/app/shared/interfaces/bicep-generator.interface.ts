export interface GenerateBicepRequest {
  infrastructureConfigId: string;
}

export interface GenerateBicepResponse {
  mainBicepUri: string;
  parametersUri: string;
  moduleUris: Record<string, string>;
}