// ─── Responses ───────────────────────────────────────────────────────────────

export interface CustomDomainResponse {
  id: string;
  resourceId: string;
  environmentName: string;
  domainName: string;
  bindingType: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface AddCustomDomainRequest {
  environmentName: string;
  domainName: string;
  bindingType: string;
}
