import { inject, Injectable } from '@angular/core';
import axios, { AxiosHeaders, CanceledError } from 'axios';
import { environment } from '../../../environments/environment';
import { MethodEnum } from '../enums/method.enum';
import { MsalAuthService } from './msal-auth.service';

@Injectable({
  providedIn: 'root',
})
export class AxiosService {
  private readonly msalAuth = inject(MsalAuthService);
  private loginRedirectInProgress = false;

  constructor() {
    const msalAuth = this.msalAuth;
    const redirectToLogin = () => this.redirectToLogin();

    axios.defaults.baseURL = environment.api_url;

    axios.interceptors.request.use(
      async function (config) {
        const token = await msalAuth.getAccessToken();
        if (!token) {
          return redirectToLogin();
        }
        config.headers = new AxiosHeaders(config.headers);
        config.headers.set('Authorization', `Bearer ${token}`);
        return config;
      },
      function (error) {
        return Promise.reject(error);
      }
    );

    axios.interceptors.response.use(
      function (response) {
        return response;
      },
      function (error) {
        if (error.response?.status === 401) {
          return redirectToLogin();
        }
        return Promise.reject(error);
      }
    );
  }

  private redirectToLogin(): Promise<never> {
    if (globalThis.location.pathname === '/login') {
      return Promise.reject(new CanceledError('Authentication required.'));
    }

    if (!this.loginRedirectInProgress) {
      this.loginRedirectInProgress = true;
      globalThis.location.replace('/login');
    }

    return new Promise<never>(() => {});
  }

  public async request$<T = unknown>(
    method: MethodEnum,
    url: string,
    data?: unknown,
    headers: Record<string, string> = {},
    isFormFile: boolean = false
  ): Promise<T> {
    const contentType = isFormFile ? 'multipart/form-data' : 'application/json';

    const response = await axios<T>({
      method,
      url,
      data,
      headers: { ...headers, 'Content-Type': contentType },
      params: method === MethodEnum.GET ? data : {},
    });

    return response.data;
  }
}
