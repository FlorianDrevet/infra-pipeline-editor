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
  isExisting?: boolean;
}

export interface CosmosDbResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: CosmosDbEnvironmentConfigResponse[];
  isExisting?: boolean;
}

export interface CreateCosmosDbRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: CosmosDbEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateCosmosDbRequest {
  name: string;
  location: string;
  environmentSettings?: CosmosDbEnvironmentConfigEntry[];
}
