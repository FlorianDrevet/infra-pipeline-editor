import { inject, Injectable } from '@angular/core';
import {
  AccountInfo,
  AuthenticationResult,
  PublicClientApplication,
  RedirectRequest,
  SilentRequest,
} from '@azure/msal-browser';
import { msalConfig, loginRequest } from '../configs/msal.config';
import { AuthenticationService } from './authentication.service';

@Injectable({ providedIn: 'root' })
export class MsalAuthService {
  private authService = inject(AuthenticationService);
  private msalInstance: PublicClientApplication;
  private initPromise: Promise<void> | null = null;

  constructor() {
    this.msalInstance = new PublicClientApplication(msalConfig);
  }

  private initialize(): Promise<void> {
    if (!this.initPromise) {
      this.initPromise = this.msalInstance
        .initialize()
        .then(() => this.msalInstance.handleRedirectPromise())
        .then((redirectResult) => {
          if (redirectResult?.account) {
            this.authService.setMsalAccount(redirectResult.account);
            return;
          }

          // Restore account from MSAL cache so auth survives page refresh
    this.authService.setAuthToken(result.accessToken);
          if (accounts.length > 0) {
            this.authService.setMsalAccount(accounts[0]);
          }
        })
        .catch((err) => {
          // Reset so the next call can retry
          this.initPromise = null;
          throw err;
        });
    }
    return this.initPromise;
  }

  public async loginPopup(): Promise<AuthenticationResult> {
    await this.initialize();
    const result = await this.msalInstance.loginPopup(loginRequest);
    this.authService.setMsalAccount(result.account);
    return result;
  }

  public async loginRedirect(redirectStartPage?: string): Promise<void> {
    await this.initialize();
    const request: RedirectRequest = {
      ...loginRequest,
      ...(redirectStartPage ? { redirectStartPage } : {}),
    };
    await this.msalInstance.loginRedirect(request);
  }

  public async getActiveAccount(): Promise<AccountInfo | null> {
    await this.initialize();
    const accounts = this.msalInstance.getAllAccounts();
    return accounts.length > 0 ? accounts[0] : null;
  }

  public async getAccessToken(): Promise<string | null> {
    await this.initialize();
    const account = await this.getActiveAccount();
    if (!account) {
      return null;
    }
    try {
      const request: SilentRequest = { ...loginRequest, account };
      const result = await this.msalInstance.acquireTokenSilent(request);
      return result.accessToken || null;
    } catch (err) {
      console.warn('MsalAuthService: silent token acquisition failed', err);
      return null;
    }
  }

  public async logout(): Promise<void> {
    await this.initialize();
    const account = await this.getActiveAccount();
    this.authService.logout();
    await this.msalInstance.logoutPopup({ account: account ?? undefined });
    this.authService.logout();
  }
}
