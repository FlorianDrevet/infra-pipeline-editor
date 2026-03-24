import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  AppSettingResponse,
  AvailableOutputsResponse,
  AddAppSettingRequest,
} from '../interfaces/app-setting.interface';

@Injectable({
  providedIn: 'root',
})
export class AppSettingService {
  private axios = inject(AxiosService);

  getByResourceId(resourceId: string): Promise<AppSettingResponse[]> {
    return this.axios.request$<AppSettingResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/app-settings`
    );
  }

  getAvailableOutputs(resourceId: string): Promise<AvailableOutputsResponse> {
    return this.axios.request$<AvailableOutputsResponse>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/available-outputs`
    );
  }

  add(resourceId: string, request: AddAppSettingRequest): Promise<AppSettingResponse> {
    return this.axios.request$<AppSettingResponse>(
      MethodEnum.POST,
      `/azure-resources/${resourceId}/app-settings`,
      request
    );
  }

  remove(resourceId: string, appSettingId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/azure-resources/${resourceId}/app-settings/${appSettingId}`
    );
  }
}
