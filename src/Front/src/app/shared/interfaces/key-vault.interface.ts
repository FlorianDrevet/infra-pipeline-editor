// ─── Responses ───────────────────────────────────────────────────────────────

export interface KeyVaultResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateKeyVaultRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  sku: string;
}

export interface UpdateKeyVaultRequest {
  name: string;
  location: string;
  sku: string;
}
