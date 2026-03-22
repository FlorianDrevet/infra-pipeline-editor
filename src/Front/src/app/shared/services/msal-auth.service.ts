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
  private readonly authService = inject(AuthenticationService);
  private readonly msalInstance: PublicClientApplication;
  private initPromise: Promise<void> | null = null;

  constructor() {
    this.msalInstance = new PublicClientApplication(msalConfig);
  }

  private initialize(): Promise<void> {
    return (this.initPromise ??= this.msalInstance
      .initialize()
      .then(() => this.msalInstance.handleRedirectPromise())
      .then((redirectResult) => {
        if (redirectResult?.account) {
          this.msalInstance.setActiveAccount(redirectResult.account);
          this.authService.setMsalAccount(redirectResult.account);
          return;
        }

        const activeAccount = this.msalInstance.getActiveAccount();
        if (activeAccount) {
          this.authService.setMsalAccount(activeAccount);
          return;
        }

        const cachedAccounts = this.msalInstance.getAllAccounts();
        const authServiceAccount = this.authService.getMsalAccount;
        const matchedAccount = this.findCachedAccount(authServiceAccount, cachedAccounts);
        if (matchedAccount) {
          this.msalInstance.setActiveAccount(matchedAccount);
          this.authService.setMsalAccount(matchedAccount);
          return;
        }

        if (cachedAccounts.length === 1) {
          const singleAccount = cachedAccounts[0];
          this.msalInstance.setActiveAccount(singleAccount);
          this.authService.setMsalAccount(singleAccount);
          return;
        }

        this.msalInstance.setActiveAccount(null);
        this.authService.setMsalAccount(null);
      })
      .catch((err) => {
        // Reset so the next call can retry
        this.initPromise = null;
        throw err;
      }));
  }

  private findCachedAccount(
    account: AccountInfo | null,
    cachedAccounts: AccountInfo[] = this.msalInstance.getAllAccounts()
  ): AccountInfo | null {
    if (!account) {
      return null;
    }

    return (
      cachedAccounts.find((cached) => cached.homeAccountId === account.homeAccountId) ??
      cachedAccounts.find((cached) => cached.localAccountId === account.localAccountId) ??
      cachedAccounts.find((cached) => cached.username === account.username) ??
      null
    );
  }

  private resolveActiveAccount(): AccountInfo | null {
    const activeAccount = this.msalInstance.getActiveAccount();
    if (activeAccount) {
      this.authService.setMsalAccount(activeAccount);
      return activeAccount;
    }

    const cachedAccounts = this.msalInstance.getAllAccounts();
    const authServiceAccount = this.authService.getMsalAccount;
    const matchedAccount = this.findCachedAccount(authServiceAccount, cachedAccounts);
    if (matchedAccount) {
      this.msalInstance.setActiveAccount(matchedAccount);
      this.authService.setMsalAccount(matchedAccount);
      return matchedAccount;
    }

    if (cachedAccounts.length === 1) {
      const singleAccount = cachedAccounts[0];
      this.msalInstance.setActiveAccount(singleAccount);
      this.authService.setMsalAccount(singleAccount);
      return singleAccount;
    }

    this.msalInstance.setActiveAccount(null);
    this.authService.setMsalAccount(null);
    return null;
  }

  public async loginPopup(): Promise<AuthenticationResult> {
    await this.initialize();
    const result = await this.msalInstance.loginPopup(loginRequest);
    this.msalInstance.setActiveAccount(result.account);
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
    return this.resolveActiveAccount();
  }

  public async getAccessToken(): Promise<string | null> {
    await this.initialize();
    const account = this.resolveActiveAccount();
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

  public async getAccessTokenForScopes(scopes: string[]): Promise<string | null> {
    await this.initialize();
    const account = this.resolveActiveAccount();
    if (!account) {
      return null;
    }
    try {
      const request: SilentRequest = { scopes, account };
      const result = await this.msalInstance.acquireTokenSilent(request);
      return result.accessToken || null;
    } catch (err) {
      console.warn('MsalAuthService: silent token acquisition failed for scopes', scopes, err);
      return null;
    }
  }

  public async logout(): Promise<void> {
    await this.initialize();
    const account = this.resolveActiveAccount();
    await this.msalInstance.logoutPopup({ account: account ?? undefined });
    this.msalInstance.setActiveAccount(null);
    this.authService.logout();
  }
}
