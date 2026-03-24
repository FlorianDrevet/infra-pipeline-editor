export interface SqlDatabaseResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  sqlServerId: string;
  collation: string;
  environmentSettings: SqlDatabaseEnvironmentConfigResponse[];
}

export interface SqlDatabaseEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  maxSizeGb: number | null;
  zoneRedundant: boolean | null;
}

export interface CreateSqlDatabaseRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sqlServerId: string;
  collation: string;
  environmentSettings?: SqlDatabaseEnvironmentConfigEntry[];
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
