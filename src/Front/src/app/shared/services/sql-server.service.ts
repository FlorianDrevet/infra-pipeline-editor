import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { DependentResourceResponse } from '../interfaces/dependent-resource.interface';

@Injectable({
  providedIn: 'root',
})
export class SqlServerService {
  private readonly axios = inject(AxiosService);

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/sql-server/${id}`);
  }

  getDependents(id: string): Promise<DependentResourceResponse[]> {
    return this.axios.request$<DependentResourceResponse[]>(
      MethodEnum.GET,
      `/sql-server/${id}/dependents`
    );
  }
}
