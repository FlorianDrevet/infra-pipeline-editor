import { inject, Injectable } from '@angular/core';

import { MsalAuthService } from './msal-auth.service';

const MicrosoftGraphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photo/$value';
const MicrosoftGraphPhotoScopes = ['User.Read'];
const AuthorizationHeaderName = 'Authorization';
const BearerTokenPrefix = 'Bearer';
const HttpStatusNoContent = 204;
const HttpStatusNotFound = 404;

@Injectable({
  providedIn: 'root',
})
export class MicrosoftGraphProfilePhotoService {
  private readonly msalAuthService = inject(MsalAuthService);

  /**
   * Returns the raw Microsoft Graph profile photo payload so callers can own
   * object URL creation and revocation.
   */
  public async getCurrentUserPhotoBlob(): Promise<Blob | null> {
    const accessToken = await this.msalAuthService.getAccessTokenForScopesSilently([...MicrosoftGraphPhotoScopes]);
    if (!accessToken) {
      return null;
    }

    try {
      const response = await globalThis.fetch(MicrosoftGraphPhotoEndpoint, {
        headers: {
          [AuthorizationHeaderName]: `${BearerTokenPrefix} ${accessToken}`,
        },
      });

      if (response.status === HttpStatusNotFound || response.status === HttpStatusNoContent) {
        return null;
      }

      if (!response.ok) {
        console.warn('MicrosoftGraphProfilePhotoService: unable to load profile photo', response.status);
        return null;
      }

      const photoBlob = await response.blob();
      if (photoBlob.size === 0) {
        return null;
      }

      return photoBlob;
    } catch (error) {
      console.warn('MicrosoftGraphProfilePhotoService: profile photo request failed', error);
      return null;
    }
  }
}