export interface SqlServerResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  version: string;
  administratorLogin: string;
  environmentSettings: SqlServerEnvironmentConfigResponse[];
  isExisting?: boolean;
}

export interface SqlServerEnvironmentConfigResponse {
  environmentName: string;
  minimalTlsVersion: string | null;
  isExisting?: boolean;
}

export interface CreateSqlServerRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  version: string;
  administratorLogin: string;
  environmentSettings?: SqlServerEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateSqlServerRequest {
  name: string;
  location: string;
  version: string;
  administratorLogin: string;
  environmentSettings?: SqlServerEnvironmentConfigEntry[];
}

export interface SqlServerEnvironmentConfigEntry {
  environmentName: string;
  minimalTlsVersion?: string | null;
}
