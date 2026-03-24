import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ContainerAppEnvironmentResponse,
  CreateContainerAppEnvironmentRequest,
  UpdateContainerAppEnvironmentRequest,
} from '../interfaces/container-app-environment.interface';

@Injectable({
  providedIn: 'root',
})
export class ContainerAppEnvironmentService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<ContainerAppEnvironmentResponse> {
    return this.axios.request$<ContainerAppEnvironmentResponse>(
      MethodEnum.GET,
      `/container-app-environment/${id}`
    );
  }

  create(request: CreateContainerAppEnvironmentRequest): Promise<ContainerAppEnvironmentResponse> {
    return this.axios.request$<ContainerAppEnvironmentResponse>(
      MethodEnum.POST,
      '/container-app-environment',
      request
    );
  }

  update(id: string, request: UpdateContainerAppEnvironmentRequest): Promise<ContainerAppEnvironmentResponse> {
    return this.axios.request$<ContainerAppEnvironmentResponse>(
      MethodEnum.PUT,
      `/container-app-environment/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/container-app-environment/${id}`);
  }
}
