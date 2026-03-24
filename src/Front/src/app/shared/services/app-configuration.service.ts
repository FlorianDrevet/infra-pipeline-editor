import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  AppConfigurationResponse,
  CreateAppConfigurationRequest,
  UpdateAppConfigurationRequest,
} from '../interfaces/app-configuration.interface';

@Injectable({
  providedIn: 'root',
})
export class AppConfigurationService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<AppConfigurationResponse> {
    return this.axios.request$<AppConfigurationResponse>(
      MethodEnum.GET,
      `/app-configuration/${id}`
    );
  }

  create(request: CreateAppConfigurationRequest): Promise<AppConfigurationResponse> {
    return this.axios.request$<AppConfigurationResponse>(
      MethodEnum.POST,
      '/app-configuration',
      request
    );
  }

  update(id: string, request: UpdateAppConfigurationRequest): Promise<AppConfigurationResponse> {
    return this.axios.request$<AppConfigurationResponse>(
      MethodEnum.PUT,
      `/app-configuration/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/app-configuration/${id}`);
  }
}
