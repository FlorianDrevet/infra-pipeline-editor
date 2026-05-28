export interface NetworkingProfileResponse {
  id: string;
  infraConfigId: string;
  mode: string;
  vnetSourceType: string;
  existingVnetResourceId: string | null;
  createNewAddressSpace: string | null;
  createNewSubnetAddressPrefix: string | null;
  privateEndpointsSubnetName: string | null;
  dnsMode: string;
  dnsHubResourceGroupId: string | null;
  dnsHubSubscriptionId: string | null;
}

export interface SetNetworkingProfileRequest {
  mode: string;
  vnetSourceType: string;
  existingVnetResourceId: string | null;
  createNewAddressSpace: string | null;
  createNewSubnetAddressPrefix: string | null;
  privateEndpointsSubnetName: string | null;
  dnsMode: string;
  dnsHubResourceGroupId: string | null;
  dnsHubSubscriptionId: string | null;
}
