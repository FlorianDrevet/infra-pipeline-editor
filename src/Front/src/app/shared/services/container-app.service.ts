import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ContainerAppResponse,
  CreateContainerAppRequest,
  UpdateContainerAppRequest,
} from '../interfaces/container-app.interface';

@Injectable({
  providedIn: 'root',
})
export class ContainerAppService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<ContainerAppResponse> {
    return this.axios.request$<ContainerAppResponse>(
      MethodEnum.GET,
      `/container-app/${id}`
    );
  }

  create(request: CreateContainerAppRequest): Promise<ContainerAppResponse> {
    return this.axios.request$<ContainerAppResponse>(
      MethodEnum.POST,
      '/container-app',
      request
    );
  }

  update(id: string, request: UpdateContainerAppRequest): Promise<ContainerAppResponse> {
    return this.axios.request$<ContainerAppResponse>(
      MethodEnum.PUT,
      `/container-app/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/container-app/${id}`);
  }
}
