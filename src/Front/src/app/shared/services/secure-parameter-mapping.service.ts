import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  SecureParameterMappingResponse,
  SetSecureParameterMappingRequest,
} from '../interfaces/secure-parameter-mapping.interface';

@Injectable({
  providedIn: 'root',
})
export class SecureParameterMappingService {
  private readonly axios = inject(AxiosService);

  getByResourceId(resourceId: string): Promise<SecureParameterMappingResponse[]> {
    return this.axios.request$<SecureParameterMappingResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/secure-parameter-mappings`
    );
  }

  set(resourceId: string, request: SetSecureParameterMappingRequest): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/azure-resources/${resourceId}/secure-parameter-mappings`,
      request
    );
  }
}
