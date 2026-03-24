import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';

@Injectable({
  providedIn: 'root',
})
export class SqlDatabaseService {
  private readonly axios = inject(AxiosService);

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/sql-database/${id}`);
  }
}
