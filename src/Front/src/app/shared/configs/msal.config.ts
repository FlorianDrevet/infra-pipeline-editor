import { BrowserCacheLocation, Configuration } from '@azure/msal-browser';
import { environment } from '../../../environments/environment';

const runtimeRedirectUri =
  typeof globalThis !== 'undefined' && globalThis.location?.origin
    ? globalThis.location.origin
    : environment.msalConfig.redirectUri;

export const msalConfig: Configuration = {
  auth: {
    clientId: environment.msalConfig.clientId,
    authority: environment.msalConfig.authority,
    redirectUri: runtimeRedirectUri,
  },
  cache: {
    cacheLocation: BrowserCacheLocation.SessionStorage,
  },
};

const apiScopes: string[] = environment.msalConfig?.apiScopes ?? [];

export const loginRequest = {
  scopes: ['openid', 'profile', 'email', ...apiScopes],
};
