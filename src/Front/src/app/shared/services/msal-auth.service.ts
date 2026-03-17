import { inject, Injectable } from '@angular/core';
import {
  AccountInfo,
  AuthenticationResult,
  PublicClientApplication,
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
        .then(() => undefined);
    }
    return this.initPromise;
  }

  public async loginPopup(): Promise<AuthenticationResult> {
    await this.initialize();
    const result = await this.msalInstance.loginPopup(loginRequest);
    this.authService.setMsalAccount(result.account);
    this.authService.setAuthToken(result.idToken);
    return result;
  }

  public async getActiveAccount(): Promise<AccountInfo | null> {
    await this.initialize();
    const accounts = this.msalInstance.getAllAccounts();
    return accounts.length > 0 ? accounts[0] : null;
  }

  public async logout(): Promise<void> {
    await this.initialize();
    const account = await this.getActiveAccount();
    await this.msalInstance.logoutPopup({ account: account ?? undefined });
  }
}
