export interface CosmosDbEnvironmentConfigEntry {
  environmentName: string;
  databaseApiType?: string | null;
  consistencyLevel?: string | null;
  maxStalenessPrefix?: number | null;
  maxIntervalInSeconds?: number | null;
  enableAutomaticFailover?: boolean | null;
  enableMultipleWriteLocations?: boolean | null;
  backupPolicyType?: string | null;
  enableFreeTier?: boolean | null;
}

export interface CosmosDbEnvironmentConfigResponse {
  environmentName: string;
  databaseApiType: string | null;
  consistencyLevel: string | null;
  maxStalenessPrefix: number | null;
  maxIntervalInSeconds: number | null;
  enableAutomaticFailover: boolean | null;
  enableMultipleWriteLocations: boolean | null;
  backupPolicyType: string | null;
  enableFreeTier: boolean | null;
}

export interface CosmosDbResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: CosmosDbEnvironmentConfigResponse[];
}

export interface CreateCosmosDbRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: CosmosDbEnvironmentConfigEntry[];
}

export interface UpdateCosmosDbRequest {
  name: string;
  location: string;
  environmentSettings?: CosmosDbEnvironmentConfigEntry[];
}
