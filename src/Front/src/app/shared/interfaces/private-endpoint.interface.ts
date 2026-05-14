// ─── Responses ───────────────────────────────────────────────────────────────

export interface PrivateEndpointConfigResponse {
  id: string;
  resourceId: string;
  subnetId: string;
  groupId: string;
  autoApproval: boolean;
  privateDnsZoneId: string | null;
  customNetworkInterfaceName: string | null;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddPrivateEndpointRequest {
  subnetId: string;
  groupId: string;
  autoApproval?: boolean;
  privateDnsZoneId?: string;
  customNetworkInterfaceName?: string;
}
