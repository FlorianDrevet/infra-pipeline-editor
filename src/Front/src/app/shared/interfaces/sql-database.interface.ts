export interface SqlDatabaseResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  sqlServerId: string;
  collation: string;
  environmentSettings: SqlDatabaseEnvironmentConfigResponse[];
  isExisting?: boolean;
}

export interface SqlDatabaseEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  maxSizeGb: number | null;
  zoneRedundant: boolean | null;
  isExisting?: boolean;
}

export interface CreateSqlDatabaseRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sqlServerId: string;
  collation: string;
  environmentSettings?: SqlDatabaseEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateSqlDatabaseRequest {
  name: string;
  location: string;
  sqlServerId: string;
  collation: string;
  environmentSettings?: SqlDatabaseEnvironmentConfigEntry[];
}

export interface SqlDatabaseEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  maxSizeGb?: number | null;
  zoneRedundant?: boolean | null;
}
