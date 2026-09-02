export interface PrivateEndpointConfigResponse {
  virtualNetworkId: string;
  subnetName: string;
  dnsMode: string;
  dnsHubResourceGroupId: string | null;
  dnsHubSubscriptionId: string | null;
}

export interface SetPrivateEndpointConfigRequest {
  virtualNetworkId: string;
  subnetName: string;
  dnsMode: string;
  dnsHubResourceGroupId?: string;
  dnsHubSubscriptionId?: string;
}
