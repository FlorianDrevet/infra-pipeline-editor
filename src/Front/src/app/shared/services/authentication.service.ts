import {computed, inject, Injectable, signal} from '@angular/core';
import {JwtHelperService} from "@auth0/angular-jwt";
import {CookieService} from "ngx-cookie-service";
import {Role} from "../enums/role.enum";
import {Router} from "@angular/router";
import {AccountInfo} from "@azure/msal-browser";

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  jwtHelper = inject(JwtHelperService)
  cookieService = inject(CookieService)
  router = inject(Router)
  private _token = signal<string | null>(null)
  private _msalAccount = signal<AccountInfo | null>(null)
  private _isAuthenticated = signal<boolean>(false)
  private _isAdmin = computed(() => {
    if (!this._isAuthenticated())
      return false;
    const decoded = this.jwtHelper.decodeToken(this._token()!);
    // Support backend JWT role claim and Azure AD roles claim (array)
    if (decoded?.role) {
      return decoded.role === Role.ADMIN;
    }
    if (Array.isArray(decoded?.roles)) {
      return decoded.roles.includes(Role.ADMIN);
    }
    return false;
  })

  constructor() {
    this._setupAuthentication()
  }

  public get getIsAuthenticated(): boolean {
    return this._isAuthenticated()
  }

  public get getIsAdmin(): boolean {
    return this._isAdmin()
  }

  public get getMsalAccount(): AccountInfo | null {
    return this._msalAccount()
  }

  public setMsalAccount(account: AccountInfo | null): void {
    this._msalAccount.set(account)
  }

  public getBearerToken(): string | null {
    const token = this._token();

    if (!this._isTokenValid())
      return null;

    return `Bearer ${token}`;
  }

  public setAuthToken(token: string): void {
    this._token.set(token)

    if (this._isTokenValid()) {
      this._isAuthenticated.set(true)
    }

    const decodedTokenDate = this.jwtHelper.getTokenExpirationDate(token);
    if (decodedTokenDate === null) {
      return;
    }

    this.cookieService.set("authentication_token", token, decodedTokenDate, "/", "", true, "Strict");
  }

  public logout() {
    this._deleteToken()
    this._msalAccount.set(null)
    this._isAuthenticated.set(false)
    this.router.navigate(['/login'])
  }

  private _setupAuthentication(): void {
    this._getAuthToken()

    if (this._isTokenValid()) {
      this._isAuthenticated.set(true)
      return;
    }
  }

  private _deleteToken(): void {
    this.cookieService.delete("authentication_token", "/");
  }

  private _isTokenValid(): boolean {
    const token = this._token()
    if (token == null || this.jwtHelper.isTokenExpired(token)) {
      this.logout()
      return false;
    }
    return true;
  }

  private _getAuthToken(): string | null {
    if (this.cookieService.check("authentication_token")) {
      const token = this.cookieService.get("authentication_token")
      this._token.set(token)
      return token;
    }
    return null;
  }
}
