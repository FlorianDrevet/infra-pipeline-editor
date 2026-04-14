import { inject, Injectable } from '@angular/core';
import { AxiosService } from '../../../shared/services/axios.service';
import { MethodEnum } from '../../../shared/enums/method.enum';
import {
  AppConfigurationKeyResponse,
  AddAppConfigurationKeyRequest,
} from '../models/app-configuration-key.interface';

@Injectable({
  providedIn: 'root',
})
export class AppConfigurationKeyService {
  private readonly axios = inject(AxiosService);

  list(appConfigurationId: string): Promise<AppConfigurationKeyResponse[]> {
    return this.axios.request$<AppConfigurationKeyResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${appConfigurationId}/configuration-keys`
    );
  }

  add(appConfigurationId: string, request: AddAppConfigurationKeyRequest): Promise<AppConfigurationKeyResponse> {
    return this.axios.request$<AppConfigurationKeyResponse>(
      MethodEnum.POST,
      `/azure-resources/${appConfigurationId}/configuration-keys`,
      request
    );
  }

  remove(appConfigurationId: string, configurationKeyId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/azure-resources/${appConfigurationId}/configuration-keys/${configurationKeyId}`
    );
  }
}
