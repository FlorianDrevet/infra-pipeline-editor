import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  WebAppResponse,
  CreateWebAppRequest,
  UpdateWebAppRequest,
} from '../interfaces/web-app.interface';

@Injectable({
  providedIn: 'root',
})
export class WebAppService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<WebAppResponse> {
    return this.axios.request$<WebAppResponse>(
      MethodEnum.GET,
      `/web-app/${id}`
    );
  }

  create(request: CreateWebAppRequest): Promise<WebAppResponse> {
    return this.axios.request$<WebAppResponse>(
      MethodEnum.POST,
      '/web-app',
      request
    );
  }

  update(id: string, request: UpdateWebAppRequest): Promise<WebAppResponse> {
    return this.axios.request$<WebAppResponse>(
      MethodEnum.PUT,
      `/web-app/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/web-app/${id}`);
  }
}
