// ─── Responses ───────────────────────────────────────────────────────────────

export interface UserAssignedIdentityResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateUserAssignedIdentityRequest {
  resourceGroupId: string;
  name: string;
  location: string;
}

export interface UpdateUserAssignedIdentityRequest {
  name: string;
  location: string;
}
