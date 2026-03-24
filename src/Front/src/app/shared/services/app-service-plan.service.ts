import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  AppServicePlanResponse,
  CreateAppServicePlanRequest,
  UpdateAppServicePlanRequest,
} from '../interfaces/app-service-plan.interface';
import { DependentResourceResponse } from '../interfaces/dependent-resource.interface';

@Injectable({
  providedIn: 'root',
})
export class AppServicePlanService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<AppServicePlanResponse> {
    return this.axios.request$<AppServicePlanResponse>(
      MethodEnum.GET,
      `/app-service-plan/${id}`
    );
  }

  create(request: CreateAppServicePlanRequest): Promise<AppServicePlanResponse> {
    return this.axios.request$<AppServicePlanResponse>(
      MethodEnum.POST,
      '/app-service-plan',
      request
    );
  }

  update(id: string, request: UpdateAppServicePlanRequest): Promise<AppServicePlanResponse> {
    return this.axios.request$<AppServicePlanResponse>(
      MethodEnum.PUT,
      `/app-service-plan/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/app-service-plan/${id}`);
  }

  getDependents(id: string): Promise<DependentResourceResponse[]> {
    return this.axios.request$<DependentResourceResponse[]>(
      MethodEnum.GET,
      `/app-service-plan/${id}/dependents`
    );
  }
}
