// ─── Responses ───────────────────────────────────────────────────────────────

export interface CustomDomainResponse {
  id: string;
  resourceId: string;
  environmentName: string;
  domainName: string;
  bindingType: string;
  dnsValidationStatus: string;
}

export interface DnsInstructionStepResponse {
  order: number;
  title: string;
  description: string;
  recordType: string | null;
  recordName: string | null;
  recordValue: string | null;
}

export interface DnsInstructionsResponse {
  domainName: string;
  dnsValidationStatus: string;
  steps: DnsInstructionStepResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddCustomDomainRequest {
  environmentName: string;
  domainName: string;
  bindingType: string;
}
