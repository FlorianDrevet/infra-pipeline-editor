import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  CreateVirtualNetworkRequest,
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
}
