// ─── Responses ───────────────────────────────────────────────────────────────

export interface UserAssignedIdentityResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateUserAssignedIdentityRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  isExisting?: boolean;
}

export interface UpdateUserAssignedIdentityRequest {
  name: string;
  location: string;
}
