import {computed, inject, Injectable, signal} from '@angular/core';
import {JwtHelperService} from "@auth0/angular-jwt";
import {CookieService} from "ngx-cookie-service";
import {Role} from "../enums/role.enum";
import {Router} from "@angular/router";

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  jwtHelper = inject(JwtHelperService)
  cookieService = inject(CookieService)
  router = inject(Router)
  private _token = signal<string | null>(null)
  private _isAuthenticated = signal<boolean>(false)
  private _isAdmin = computed(() => {
    if (!this._isAuthenticated())
      return false;
    return this.jwtHelper.decodeToken(this._token()!).role === Role.ADMIN
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
