export interface SqlServerResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  version: string;
  administratorLogin: string;
  environmentSettings: SqlServerEnvironmentConfigResponse[];
}

export interface SqlServerEnvironmentConfigResponse {
  environmentName: string;
  minimalTlsVersion: string | null;
}

export interface CreateSqlServerRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  version: string;
  administratorLogin: string;
  environmentSettings?: SqlServerEnvironmentConfigEntry[];
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
