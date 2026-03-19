import { inject, Injectable } from '@angular/core';
import axios, { AxiosHeaders } from 'axios';
import { environment } from '../../../environments/environment';
import { MethodEnum } from '../enums/method.enum';
import { MsalAuthService } from './msal-auth.service';

@Injectable({
  providedIn: 'root',
})
export class AxiosService {
  private readonly msalAuth = inject(MsalAuthService);

  constructor() {
    const msalAuth = this.msalAuth;

    axios.defaults.baseURL = environment.api_url;

    axios.interceptors.request.use(
      async function (config) {
        const token = await msalAuth.getAccessToken();
        if (!token) {
          throw new Error('No active session. Please sign in.');
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
          // Redirect to login page instead of triggering a popup-based logout,
          // which may be blocked when initiated from a network callback.
          globalThis.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
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
