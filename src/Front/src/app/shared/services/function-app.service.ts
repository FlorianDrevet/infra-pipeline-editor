import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  FunctionAppResponse,
  CreateFunctionAppRequest,
  UpdateFunctionAppRequest,
} from '../interfaces/function-app.interface';

@Injectable({
  providedIn: 'root',
})
export class FunctionAppService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<FunctionAppResponse> {
    return this.axios.request$<FunctionAppResponse>(
      MethodEnum.GET,
      `/function-app/${id}`
    );
  }

  create(request: CreateFunctionAppRequest): Promise<FunctionAppResponse> {
    return this.axios.request$<FunctionAppResponse>(
      MethodEnum.POST,
      '/function-app',
      request
    );
  }

  update(id: string, request: UpdateFunctionAppRequest): Promise<FunctionAppResponse> {
    return this.axios.request$<FunctionAppResponse>(
      MethodEnum.PUT,
      `/function-app/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/function-app/${id}`);
  }
}
