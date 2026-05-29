import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  AddSubnetRequest,
  CreateVirtualNetworkRequest,
  UpdateSubnetRequest,
  UpdateVirtualNetworkRequest,
  VirtualNetworkResponse,
} from '../interfaces/virtual-network.interface';

const VIRTUAL_NETWORK_ROUTE = '/virtual-network';

@Injectable({
  providedIn: 'root',
})
export class VirtualNetworkService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.GET,
      `${VIRTUAL_NETWORK_ROUTE}/${id}`
    );
  }

  create(request: CreateVirtualNetworkRequest): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.POST,
      VIRTUAL_NETWORK_ROUTE,
      request
    );
  }

  update(id: string, request: UpdateVirtualNetworkRequest): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.PUT,
      `${VIRTUAL_NETWORK_ROUTE}/${id}`,
      request
    );
  }

  addSubnet(vnetId: string, request: AddSubnetRequest): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.POST,
      `${VIRTUAL_NETWORK_ROUTE}/${vnetId}/subnets`,
      request
    );
  }

  updateSubnet(vnetId: string, subnetId: string, request: UpdateSubnetRequest): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.PUT,
      `${VIRTUAL_NETWORK_ROUTE}/${vnetId}/subnets/${subnetId}`,
      request
    );
  }

  removeSubnet(vnetId: string, subnetId: string): Promise<VirtualNetworkResponse> {
    return this.axios.request$<VirtualNetworkResponse>(
      MethodEnum.DELETE,
      `${VIRTUAL_NETWORK_ROUTE}/${vnetId}/subnets/${subnetId}`
    );
  }
}
