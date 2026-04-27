import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  CustomDomainResponse,
  AddCustomDomainRequest,
} from '../interfaces/custom-domain.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomDomainService {
  private readonly axios = inject(AxiosService);

  getByResourceId(resourceId: string): Promise<CustomDomainResponse[]> {
    return this.axios.request$<CustomDomainResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/custom-domains`
    );
  }

  add(resourceId: string, request: AddCustomDomainRequest): Promise<CustomDomainResponse> {
    return this.axios.request$<CustomDomainResponse>(
      MethodEnum.POST,
      `/azure-resources/${resourceId}/custom-domains`,
      request
    );
  }

  remove(resourceId: string, customDomainId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/azure-resources/${resourceId}/custom-domains/${customDomainId}`
    );
  }
}
