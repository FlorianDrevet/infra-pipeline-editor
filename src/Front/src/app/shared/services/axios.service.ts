import { inject, Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import { MethodEnum } from '../enums/method.enum';
import { Router } from '@angular/router';
import { AuthenticationService } from './authentication.service';

@Injectable({
  providedIn: 'root',
})
export class AxiosService {
  private auth = inject(AuthenticationService);
  private router = inject(Router);

  constructor() {
    const auth = this.auth;
    const router = this.router;

    axios.defaults.baseURL = environment.api_url;

    axios.interceptors.request.use(
      function (config) {
        if (config.url === '/auth/login') {
          return config;
        }

        const token = auth.getBearerToken();

        if (token) {
          config.headers['Authorization'] = token;
        } else {
          router.navigate(['login']).then(null);
          return Promise.reject('No token found');
        }

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
          auth.logout();
          router.navigate(['login']).then(null);
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
