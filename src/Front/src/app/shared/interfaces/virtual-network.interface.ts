// ─── Environment Settings ────────────────────────────────────────────────────

export interface VirtualNetworkEnvironmentConfigEntry {
  environmentName: string;
  addressSpaces: string[];
  dnsServers?: string[];
  enableDdosProtection: boolean;
}

export interface VirtualNetworkEnvironmentConfigResponse {
  environmentName: string;
  addressSpaces: string[];
  dnsServers: string[] | null;
  enableDdosProtection: boolean;
}

// ─── Responses ───────────────────────────────────────────────────────────────

export interface SubnetResponse {
  id: string;
  name: string;
  addressPrefix: string;
  delegation: string | null;
  serviceEndpoints: string[];
  privateEndpointNetworkPolicies: string;
  nsgId: string | null;
}

export interface VirtualNetworkResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings: VirtualNetworkEnvironmentConfigResponse[];
  subnets: SubnetResponse[];
  isExisting: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateVirtualNetworkRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: VirtualNetworkEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateVirtualNetworkRequest {
  name: string;
  location: string;
  environmentSettings?: VirtualNetworkEnvironmentConfigEntry[];
}

// ─── Subnet Requests ─────────────────────────────────────────────────────────

export interface AddSubnetRequest {
  name: string;
  addressPrefix: string;
  delegation?: string | null;
  serviceEndpoints?: string[];
  privateEndpointNetworkPolicies: string;
  nsgId?: string | null;
}

export interface UpdateSubnetRequest {
  name: string;
  addressPrefix: string;
  delegation?: string | null;
  serviceEndpoints?: string[];
  privateEndpointNetworkPolicies: string;
  nsgId?: string | null;
}
