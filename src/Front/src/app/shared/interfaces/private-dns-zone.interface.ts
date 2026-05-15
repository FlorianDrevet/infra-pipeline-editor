// ─── Responses ───────────────────────────────────────────────────────────────

export interface VirtualNetworkLinkResponse {
  id: string;
  virtualNetworkId: string;
  enableAutoRegistration: boolean;
}

export interface PrivateDnsZoneResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  virtualNetworkLinks: VirtualNetworkLinkResponse[];
  isExisting: boolean;
}
