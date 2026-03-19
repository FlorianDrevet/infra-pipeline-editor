import { inject, Injectable, signal } from '@angular/core';
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

  // Signals
  resourceGroups = signal<ResourceGroupResponse[]>([]);
  isLoadingResourceGroups = signal(false);

  loadResourceGroups(configId: string): void {
    this.isLoadingResourceGroups.set(true);

    // For now, return empty array - will be connected to actual API endpoint
    // when the backend has the proper endpoint
    this.resourceGroups.set([]);
    this.isLoadingResourceGroups.set(false);
  }

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
