import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { DependentResourceResponse } from '../interfaces/dependent-resource.interface';
import {
  SqlServerResponse,
  CreateSqlServerRequest,
  UpdateSqlServerRequest,
} from '../interfaces/sql-server.interface';

@Injectable({
  providedIn: 'root',
})
export class SqlServerService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<SqlServerResponse> {
    return this.axios.request$<SqlServerResponse>(MethodEnum.GET, `/sql-server/${id}`);
  }

  create(request: CreateSqlServerRequest): Promise<SqlServerResponse> {
    return this.axios.request$<SqlServerResponse>(MethodEnum.POST, '/sql-server', request);
  }

  update(id: string, request: UpdateSqlServerRequest): Promise<SqlServerResponse> {
    return this.axios.request$<SqlServerResponse>(MethodEnum.PUT, `/sql-server/${id}`, request);
  }

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
