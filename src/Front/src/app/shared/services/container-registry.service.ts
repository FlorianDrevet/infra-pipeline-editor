import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ContainerRegistryResponse,
  CreateContainerRegistryRequest,
  UpdateContainerRegistryRequest,
  CheckAcrPullAccessResponse,
} from '../interfaces/container-registry.interface';

@Injectable({
  providedIn: 'root',
})
export class ContainerRegistryService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<ContainerRegistryResponse> {
    return this.axios.request$<ContainerRegistryResponse>(
      MethodEnum.GET,
      `/container-registry/${id}`
    );
  }

  create(request: CreateContainerRegistryRequest): Promise<ContainerRegistryResponse> {
    return this.axios.request$<ContainerRegistryResponse>(
      MethodEnum.POST,
      '/container-registry',
      request
    );
  }

  update(id: string, request: UpdateContainerRegistryRequest): Promise<ContainerRegistryResponse> {
    return this.axios.request$<ContainerRegistryResponse>(
      MethodEnum.PUT,
      `/container-registry/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/container-registry/${id}`);
  }

  checkAcrPullAccess(resourceId: string, containerRegistryId: string): Promise<CheckAcrPullAccessResponse> {
    return this.axios.request$<CheckAcrPullAccessResponse>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/check-acr-pull-access/${containerRegistryId}`
    );
  }
}
