import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  SqlDatabaseResponse,
  CreateSqlDatabaseRequest,
  UpdateSqlDatabaseRequest,
} from '../interfaces/sql-database.interface';

@Injectable({
  providedIn: 'root',
})
export class SqlDatabaseService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<SqlDatabaseResponse> {
    return this.axios.request$<SqlDatabaseResponse>(MethodEnum.GET, `/sql-database/${id}`);
  }

  create(request: CreateSqlDatabaseRequest): Promise<SqlDatabaseResponse> {
    return this.axios.request$<SqlDatabaseResponse>(MethodEnum.POST, '/sql-database', request);
  }

  update(id: string, request: UpdateSqlDatabaseRequest): Promise<SqlDatabaseResponse> {
    return this.axios.request$<SqlDatabaseResponse>(MethodEnum.PUT, `/sql-database/${id}`, request);
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/sql-database/${id}`);
  }
}
