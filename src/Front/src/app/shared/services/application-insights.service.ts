import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ApplicationInsightsResponse,
  CreateApplicationInsightsRequest,
  UpdateApplicationInsightsRequest,
} from '../interfaces/application-insights.interface';

@Injectable({
  providedIn: 'root',
})
export class ApplicationInsightsService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<ApplicationInsightsResponse> {
    return this.axios.request$<ApplicationInsightsResponse>(
      MethodEnum.GET,
      `/application-insights/${id}`
    );
  }

  create(request: CreateApplicationInsightsRequest): Promise<ApplicationInsightsResponse> {
    return this.axios.request$<ApplicationInsightsResponse>(
      MethodEnum.POST,
      '/application-insights',
      request
    );
  }

  update(id: string, request: UpdateApplicationInsightsRequest): Promise<ApplicationInsightsResponse> {
    return this.axios.request$<ApplicationInsightsResponse>(
      MethodEnum.PUT,
      `/application-insights/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/application-insights/${id}`);
  }
}
