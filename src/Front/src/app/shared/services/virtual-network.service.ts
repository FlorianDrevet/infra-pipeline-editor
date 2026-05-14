import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  CreateVirtualNetworkRequest,
  VirtualNetworkResponse,
} from '../interfaces/virtual-network.interface';

@Injectable({
  providedIn: 'root',
})
export class VirtualNetworkService {
  private readonly axios = inject(AxiosService);

  getByResourceGroupId(resourceGroupId: string): Promise<VirtualNetworkResponse[]> {
    return this.axios.request$<VirtualNetworkResponse[]>(
      MethodEnum.GET,
      `/resource-groups/${resourceGroupId}/virtual-networks`
    );
  }

  getById(id: string): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.GET,
      `/virtual-networks/${id}`
    );
  }

  create(resourceGroupId: string, request: CreateVirtualNetworkRequest): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.POST,
      `/resource-groups/${resourceGroupId}/virtual-networks`,
      request
    );
  }
}
