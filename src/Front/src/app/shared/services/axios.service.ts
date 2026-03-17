import { inject, Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import { MethodEnum } from '../enums/method.enum';
import { MsalAuthService } from './msal-auth.service';

@Injectable({
  providedIn: 'root',
})
export class AxiosService {
  private msalAuth = inject(MsalAuthService);

  constructor() {
    const msalAuth = this.msalAuth;

    axios.defaults.baseURL = environment.api_url;

    axios.interceptors.request.use(
      async function (config) {
        const token = await msalAuth.getAccessToken();
        if (!token) {
          return Promise.reject(new Error('No active session. Please sign in.'));
        }
        config.headers['Authorization'] = `Bearer ${token}`;
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
        if (error.response && error.response.status === 401) {
          msalAuth.logout().catch(() => undefined);
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
