import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  LogAnalyticsWorkspaceResponse,
  CreateLogAnalyticsWorkspaceRequest,
  UpdateLogAnalyticsWorkspaceRequest,
} from '../interfaces/log-analytics-workspace.interface';

@Injectable({
  providedIn: 'root',
})
export class LogAnalyticsWorkspaceService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<LogAnalyticsWorkspaceResponse> {
    return this.axios.request$<LogAnalyticsWorkspaceResponse>(
      MethodEnum.GET,
      `/log-analytics-workspace/${id}`
    );
  }

  create(request: CreateLogAnalyticsWorkspaceRequest): Promise<LogAnalyticsWorkspaceResponse> {
    return this.axios.request$<LogAnalyticsWorkspaceResponse>(
      MethodEnum.POST,
      '/log-analytics-workspace',
      request
    );
  }

  update(id: string, request: UpdateLogAnalyticsWorkspaceRequest): Promise<LogAnalyticsWorkspaceResponse> {
    return this.axios.request$<LogAnalyticsWorkspaceResponse>(
      MethodEnum.PUT,
      `/log-analytics-workspace/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/log-analytics-workspace/${id}`);
  }
}
