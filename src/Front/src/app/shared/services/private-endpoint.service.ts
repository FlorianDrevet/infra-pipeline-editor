import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  AddPrivateEndpointRequest,
  PrivateEndpointConfigResponse,
} from '../interfaces/private-endpoint.interface';

@Injectable({
  providedIn: 'root',
})
export class PrivateEndpointService {
  private readonly axios = inject(AxiosService);

  getByResourceId(resourceId: string): Promise<PrivateEndpointConfigResponse[]> {
    return this.axios.request$<PrivateEndpointConfigResponse[]>(
      MethodEnum.GET,
      `/resources/${resourceId}/private-endpoints`
    );
  }

  add(resourceId: string, request: AddPrivateEndpointRequest): Promise<PrivateEndpointConfigResponse> {
    return this.axios.request$<PrivateEndpointConfigResponse>(
      MethodEnum.POST,
      `/resources/${resourceId}/private-endpoints`,
      request
    );
  }

  update(resourceId: string, peId: string, request: AddPrivateEndpointRequest): Promise<PrivateEndpointConfigResponse> {
    return this.axios.request$<PrivateEndpointConfigResponse>(
      MethodEnum.PUT,
      `/resources/${resourceId}/private-endpoints/${peId}`,
      request
    );
  }

  remove(resourceId: string, peId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/resources/${resourceId}/private-endpoints/${peId}`
    );
  }
}
