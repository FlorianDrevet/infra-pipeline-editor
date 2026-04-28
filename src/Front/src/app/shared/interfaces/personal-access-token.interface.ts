export interface PersonalAccessTokenResponse {
  id: string;
  name: string;
  tokenPrefix: string;
  expiresAt: string | null;
  createdAt: string;
  lastUsedAt: string | null;
  isRevoked: boolean;
}

export interface CreatedPersonalAccessTokenResponse {
  token: PersonalAccessTokenResponse;
  plainTextToken: string;
}

export interface CreatePersonalAccessTokenRequest {
  name: string;
  expiresAt: string | null;
}
