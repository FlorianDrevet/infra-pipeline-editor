import { BrowserCacheLocation, Configuration } from '@azure/msal-browser';
import { environment } from '../../../environments/environment';

export const msalConfig: Configuration = {
  auth: {
    clientId: environment.msalConfig.clientId,
    authority: environment.msalConfig.authority,
    redirectUri: environment.msalConfig.redirectUri,
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage,
  },
};

export const loginRequest = {
  scopes: ['openid', 'profile', 'email'],
};
