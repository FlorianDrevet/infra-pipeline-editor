import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ResourceGroupResponse,
  AzureResourceResponse,
  CreateResourceGroupRequest,
} from '../interfaces/resource-group.interface';

@Injectable({
  providedIn: 'root',
})
export class ResourceGroupService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<ResourceGroupResponse> {
    return this.axios.request$<ResourceGroupResponse>(
      MethodEnum.GET,
      `/resource-group/${id}`
    );
  }

  getResources(id: string): Promise<AzureResourceResponse[]> {
    return this.axios.request$<AzureResourceResponse[]>(
      MethodEnum.GET,
      `/resource-group/${id}/resources`
    );
  }

  create(request: CreateResourceGroupRequest): Promise<ResourceGroupResponse> {
    return this.axios.request$<ResourceGroupResponse>(
      MethodEnum.POST,
      '/resource-group',
      request
    );
  }
}
