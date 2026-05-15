// ─── Responses ───────────────────────────────────────────────────────────────

export interface NsgRuleResponse {
  id: string;
  name: string;
  priority: number;
  direction: string;
  access: string;
  protocol: string;
  sourceAddressPrefix: string;
  destinationAddressPrefix: string;
  sourcePortRange: string;
  destinationPortRange: string;
}

export interface NetworkSecurityGroupResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  securityRules: NsgRuleResponse[];
  isExisting: boolean;
}
