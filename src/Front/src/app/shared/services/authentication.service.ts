import { Injectable, signal } from '@angular/core';
import { AccountInfo } from '@azure/msal-browser';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  private _msalAccount = signal<AccountInfo | null>(null);
  private _isAuthenticated = signal<boolean>(false);

  public get getIsAuthenticated(): boolean {
    return this._isAuthenticated();
  }

  public get getMsalAccount(): AccountInfo | null {
    return this._msalAccount();
  }

  public setMsalAccount(account: AccountInfo | null): void {
    this._msalAccount.set(account);
    this._isAuthenticated.set(account !== null);
  }

  public logout(): void {
    this._msalAccount.set(null);
    this._isAuthenticated.set(false);
  }
}
